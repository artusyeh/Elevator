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
        float pitch;
        float vertical;
        public void SyncLookPitch(){pitch=eyes.transform.localEulerAngles.x;if(pitch>180)pitch-=360;}
        CharacterController controller;
        void Awake() { controller = GetComponent<CharacterController>(); }
        void OnEnable() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        void Update()
        {
            var k = Keyboard.current; var m = Mouse.current;
            if (k == null || m == null || eyes == null) { Step(Vector2.zero, Time.deltaTime, false); return; }
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
            if (controller == null) controller = GetComponent<CharacterController>();
            input = Vector2.ClampMagnitude(input, 1);
            vertical = controller.isGrounded && vertical < 0 ? -2 : vertical - 20 * dt;
            Vector3 velocity = (transform.right * input.x + transform.forward * input.y) * speed * (sprint ? 1.55f : 1);
            velocity.y = vertical; controller.Move(velocity * dt);
        }
    }
}
