using UnityEngine;
using UnityEngine.InputSystem;

namespace AlleyStudy
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AlleyWalker : MonoBehaviour
    {
        public Camera eyes;
        public float speed = 2.2f;
        public float sensitivity = 0.085f;
        [HideInInspector] public bool phoneInterfaceOpen;
        [HideInInspector] public bool seated;
        [Range(.1f,1)]public float phoneWalkMultiplier=.65f;
        [Header("Crouch (hold Ctrl)")]
        public float crouchHeight=1.12f;
        public float crouchEyeHeight=.98f;
        [Range(.1f,1)]public float crouchSpeedMultiplier=.5f;
        public float crouchTransitionSpeed=10;
        public bool IsCrouching=>controller!=null&&controller.height<standingHeight-.025f;
        float standingHeight,standingEyeHeight,bodyBottom;
        readonly Collider[] crouchHits=new Collider[16];
        float pitch;
        float vertical;
        public void SyncLookPitch(){pitch=eyes.transform.localEulerAngles.x;if(pitch>180)pitch-=360;}
        CharacterController controller;
        void Awake() { controller = GetComponent<CharacterController>();standingHeight=controller.height;bodyBottom=controller.center.y-controller.height*.5f;standingEyeHeight=eyes.transform.localPosition.y; }
        public void TickCrouch(bool held,float dt)
        {
            if(seated||controller==null||!controller.enabled)return;
            if(!held&&IsCrouching)
            {
                Vector3 bottom=transform.position+Vector3.up*(bodyBottom+controller.radius+.015f);
                Vector3 top=transform.position+Vector3.up*(bodyBottom+standingHeight-controller.radius);
                int count=Physics.OverlapCapsuleNonAlloc(bottom,top,controller.radius*.94f,crouchHits,~0,QueryTriggerInteraction.Ignore);
                if(count==crouchHits.Length)held=true;
                for(int i=0;i<count;i++)if(crouchHits[i]!=null&&!crouchHits[i].transform.IsChildOf(transform)){held=true;break;}
            }
            float blend=1-Mathf.Exp(-Mathf.Max(1,crouchTransitionSpeed)*dt);
            controller.height=Mathf.Lerp(controller.height,held?Mathf.Max(controller.radius*2+.02f,crouchHeight):standingHeight,blend);
            var center=controller.center;center.y=bodyBottom+controller.height*.5f;controller.center=center;
            var eye=eyes.transform.localPosition;eye.y=Mathf.Lerp(eye.y,held?crouchEyeHeight:standingEyeHeight,blend);eyes.transform.localPosition=eye;
        }
        void OnEnable() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        void Update()
        {
            var narrativeUi=GetComponent<InteractionScanner>();
            if(StoryDirector.Active!=null&&StoryDirector.Active.Opening||narrativeUi!=null&&(narrativeUi.documentOpen||narrativeUi.cutscenePlaying||narrativeUi.menuOpen)){Step(Vector2.zero,Time.deltaTime,false);return;}
            var k = Keyboard.current; var m = Mouse.current;
            if (k == null || m == null || eyes == null) { Step(Vector2.zero, Time.deltaTime, false); return; }
            if(Cursor.lockState==CursorLockMode.Locked||phoneInterfaceOpen)TickCrouch(k.leftCtrlKey.isPressed||k.rightCtrlKey.isPressed,Time.deltaTime);
            if(phoneInterfaceOpen)
            {
                Vector2 phoneMove=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
                Step(phoneMove,Time.deltaTime,false);return;
            }
            if (k.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            if (m.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            if (Cursor.lockState != CursorLockMode.Locked) { Step(Vector2.zero, Time.deltaTime, false); return; }
            Vector2 delta = m.delta.ReadValue() * sensitivity;
            transform.Rotate(0, delta.x, 0); pitch = Mathf.Clamp(pitch - delta.y, -80, 80);
            eyes.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            Vector2 move = new Vector2((k.dKey.isPressed ? 1 : 0)-(k.aKey.isPressed ? 1 : 0),(k.wKey.isPressed ? 1 : 0)-(k.sKey.isPressed ? 1 : 0));
            Step(move, Time.deltaTime, k.leftShiftKey.isPressed);
        }
        public void Step(Vector2 input, float dt, bool sprint)
        {
            if(seated)return;
            if (controller == null) controller = GetComponent<CharacterController>();
            input = Vector2.ClampMagnitude(input, 1);
            vertical = controller.isGrounded && vertical < 0 ? -2 : vertical - 20 * dt;
            Vector3 velocity = (transform.right * input.x + transform.forward * input.y) * speed * (phoneInterfaceOpen?phoneWalkMultiplier:1) * (IsCrouching?crouchSpeedMultiplier:!phoneInterfaceOpen&&sprint?1.55f:1);
            velocity.y = vertical; controller.Move(velocity * dt);
        }
    }
}
