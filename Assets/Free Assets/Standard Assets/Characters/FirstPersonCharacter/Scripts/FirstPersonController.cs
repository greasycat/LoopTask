using System;
using System.Collections;
using System.Collections.Generic;
using Landmarks.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] public float m_WalkSpeed; //MJS 10/24/2018 Made public for speed modifier
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;

        [SerializeField]
        private AudioClip[] m_FootstepSounds; // an array of footstep sounds that will be randomly selected from.

        [SerializeField] private AudioClip m_JumpSound; // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound; // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        private bool _autoModeEnabled;

        private Coroutine _coroutine;


        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
            m_MouseLook.Init(transform, m_Camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            // RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }

            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            if (!_autoModeEnabled)
            {
                HandleManualMovement();
            }
        }

        private void HandleManualMovement()
        {
            GetInput(out var speed);
            var desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
            // always move along the camera forward as it is the direction that it being aimed at
            // get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out var hitInfo,
                m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }


            if (m_CharacterController.enabled)
            {
                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
            }

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);
            //
            m_MouseLook.UpdateCursorLock();
        }

        public void TestAutoMode(Vector3 loopPosition,
            Vector3 startingPosition, float stoppingAngle, bool counterclockwise,
            float walkingSpeed, float turningSpeed, float loopingSpeed, HUD hud, float waitTime = 2f,
            bool recenter = false
        )
        {
            _autoModeEnabled = true;
            var startingFacingDirection = (startingPosition - Vector3.zero).normalized;
            Vector3 readyFacingDirection;
            if (counterclockwise)
            {
                readyFacingDirection = Quaternion.AngleAxis(90, Vector3.down) * startingFacingDirection;
            }
            else
            {
                readyFacingDirection = Quaternion.AngleAxis(90, Vector3.up) * startingFacingDirection;
            }

            StopAutoCoroutine();
            _coroutine = StartCoroutine(RunALoop(startingPosition, walkingSpeed,
                readyFacingDirection, turningSpeed,
                counterclockwise, stoppingAngle, loopPosition, loopingSpeed, hud, waitTime, recenter));
        }

        public IEnumerator TeleportAction(LM_TeleportAction action)
        {
            m_CharacterController.transform.position = action.destination;
            yield return null;
        }

        public IEnumerator WalkToAction(LM_WalkToAction action)
        {
            Debug.Log("Walk to action");
            var direction = action.destination - transform.position;
            yield return TurnTo(direction, action.speed);
            yield return WalkTo(action.destination, action.speed);
            yield return null;
        }

        public IEnumerator LoopAction(LM_LoopAction action)
        {
            var counterClockwise = action.loopDirection == "counterclockwise";
            Vector3 readyFacingDirection;
            if (counterClockwise)
            {
                readyFacingDirection =
                    Quaternion.AngleAxis(90, Vector3.down) * (transform.position - action.loopCenter);
            }
            else
            {
                readyFacingDirection = Quaternion.AngleAxis(90, Vector3.up) * (transform.position - action.loopCenter);
            }
            
            yield return Loop(action.loopCenter, action.loopRadius, action.loopAngle, counterClockwise,
                action.loopSpeed);
        }
        
        public IEnumerator TriggerAction(LM_TriggerAction action, HUD hud)
        {
            yield return null;
            hud.OnActionClick();
        }
        
        public IEnumerator PauseAction(LM_PauseAction action)
        {
            yield return new WaitForSeconds(action.duration);
        }


        public void StopAutoMode()
        {
            _autoModeEnabled = false;
        }

        private void StopAutoCoroutine()
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }

        // private IEnumerator WalkAndTurn

        private IEnumerator TurnTo(Vector3 targetRotation, float speed)
        {
            Debug.Log("Turn to");
            var rotation = Quaternion.LookRotation(targetRotation);
            var startRot = transform.rotation;
            // get the shortest angle
            var angle = Quaternion.Angle(startRot, rotation);
            var duration = angle / (speed * 10);
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.rotation = Quaternion.Slerp(startRot, rotation, t / duration);
                yield return null;
            }

            transform.rotation = rotation;
        }

        private IEnumerator WalkTo(Vector3 targetPosition, float speed)
        {
            Debug.Log("walk to");
            var startPosition = transform.position;
            // transform.rotation = Quaternion.LookRotation(facingDirection);
            var duration = Vector3.Distance(startPosition, targetPosition) / speed * 2f;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t / duration);
                yield return null;
            }

            transform.position = targetPosition;
        }

        private IEnumerator Loop(
            Vector3 loopCenterPosition,
            float radius,
            float differenceAngle,
            bool counterclockwise,
            float speed
        )
        {
            // calculate the current angle, 180 to -180
            var currentAngle = Mathf.Atan2(transform.position.z - loopCenterPosition.z,
                transform.position.x - loopCenterPosition.x);
            // convert to 0 to 360
            if (currentAngle < 0) currentAngle += 2 * Mathf.PI;

            float finalAngle;
            if (counterclockwise)
            {
                finalAngle = currentAngle + differenceAngle;
            }
            else
            {
                finalAngle = currentAngle - differenceAngle;
            }

            // var radius = Vector3.Distance(transform.position, loopCenterPosition);

            Debug.Log($"Current angle: {currentAngle * 180 / Mathf.PI}");
            Debug.Log($"stopping angle: {differenceAngle * 180 / Mathf.PI}");
            Debug.Log($"Final angle: {finalAngle * 180 / Mathf.PI}");
            for (var w = 0f; w < differenceAngle; w += Time.deltaTime * speed * 0.1f)
            {
                var angle = currentAngle + (counterclockwise ? 1 : -1) * w;
                var x = Mathf.Cos(angle) * radius + loopCenterPosition.x;
                var z = Mathf.Sin(angle) * radius + loopCenterPosition.z;
                var y = loopCenterPosition.y;
                var newPosition = new Vector3(x, y, z);
                var direction = newPosition - m_CharacterController.transform.position;
                m_CharacterController.transform.position = newPosition;
                m_CharacterController.transform.rotation = Quaternion.LookRotation(direction);
                yield return null;
            }
        }

        private IEnumerator RunALoop(Vector3 startingPositions, float walkingSpeed,
            Vector3 readyFacingDirection, float turningSpeed,
            bool counterclockwise, float stoppingAngle, Vector3 loopPosition, float rotationSpeed,
            HUD hud, float waitTime = 2, bool restartAtCenter = true)
        {
            if (restartAtCenter)
            {
                var direction = loopPosition - transform.position;
                yield return TurnTo(direction, turningSpeed);
                yield return WalkTo(loopPosition, walkingSpeed);
            }

            // yield return Turn(startingPositions - transform.position, turningSpeed);
            // yield return WalkTo(startingPositions, walkingSpeed);
            // yield return Turn(readyFacingDirection, turningSpeed);
            // yield return new WaitForSeconds(waitTime);
            // hud.OnActionClick();
            // yield return Loop(counterclockwise, stoppingAngle, loopPosition, rotationSpeed);
            // hud.OnActionClick();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude +
                                (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                               Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }

            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }

            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                        (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }

            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }

            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }

        // Access the private MouseLook variable to reset it when reorineting the player manually
        public void ResetMouselook()
        {
            m_MouseLook.Init(transform, m_Camera.transform);
        }
    }
}