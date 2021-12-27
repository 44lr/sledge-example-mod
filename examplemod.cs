using SledgeLib; // use SledgeLib's namespace
using System.Numerics; // use Numerics for Vectors

/*
 *  class that contains our init function
 *  keep this in mind for when writing the mod's .info.json
 */
public class examplemod
{
    // variables that will be used later in the mod
    static bool m_GodEnabled = false;
    static bool m_NoclipEnabled = false;

    static bool m_ReduceSpeed = false;
    static bool m_IncreaseSpeed = false;

    static Vector3 m_NoclipExitSpeed; // speed that will be applied to the player once they stop using noclip
    static Transform m_NoclipTransform; // transform that will store the camera's position while the player is noclipping
    static float m_NoclipSpeed = .25f; // speed at which the player will fly

    /*
     * these functions will be called every time the key is pressed, not released
     */
    static void ToggleNoclip()
    {
        m_NoclipEnabled = !m_NoclipEnabled;
        Log.General("Noclip {0}", m_NoclipEnabled ? "enabled" : "disabled");

        if (m_NoclipEnabled)
        {
            m_NoclipTransform = Player.GetCameraTransform(); // store the camera's last transform
        } else
        {
            Player.SetPosition(m_NoclipTransform.Position - new Vector3(0, 1.7f, 0)); // set the player's position to where the camera's position last was (minus the camera offset)
            Player.SetVelocity(m_NoclipExitSpeed); // apply the exit velocity to the player
        }
    }
    static void ToggleGod()
    {
        m_GodEnabled = !m_GodEnabled;
        Log.General("God mode {0}", m_GodEnabled ? "enabled" : "disabled");
    }
    
    /*
     *  these functions will be called when the key they're bound to is pressed or released
     *  bKeyDown will be true if the key was pressed, or false if it was released
     *  (it'll also be called multiple times while it's pressed)
     */
    static void UpdateReduceSpeed(bool bKeyDown) { m_ReduceSpeed = bKeyDown;  }
    static void UpdateIncreaseSpeed(bool bKeyDown) { m_IncreaseSpeed = bKeyDown; }

    /*
     * this function will be called every time the player is done updating (camera position updates, health updates, etc)
     */
    static void OnPostPlayerUpdate()
    {
        // if god mode is enabled, constantly heal the player
        if (m_GodEnabled)
            Player.SetHealth(1);

        if (m_NoclipEnabled)
        {
            // Set the player's velocity to 0 to avoid taking fall damage
            Player.SetVelocity(new Vector3(0, 0, 0));

            // store the camera's rotation since we won't be modifying it and we'll need it for setting the transform later
            m_NoclipTransform.Rotation = Player.GetCameraTransform().Rotation;

            // read the player's movement input
            Vector2 vInput = Player.GetMovementInput();

            // calculate the forward vector to move the player forwards and backwards
            Vector3 vFwdVec;
            vFwdVec.X = 2 * (m_NoclipTransform.Rotation.X * m_NoclipTransform.Rotation.Z + m_NoclipTransform.Rotation.W * m_NoclipTransform.Rotation.Y);
            vFwdVec.Y = 2 * (m_NoclipTransform.Rotation.Y * m_NoclipTransform.Rotation.Z - m_NoclipTransform.Rotation.W * m_NoclipTransform.Rotation.X);
            vFwdVec.Z = 1 - 2 * (m_NoclipTransform.Rotation.X * m_NoclipTransform.Rotation.X + m_NoclipTransform.Rotation.Y * m_NoclipTransform.Rotation.Y);

            // multiply the forward vector by the input (to determine whether to move forwards or backwards)
            vFwdVec *= -vInput.X;
            // multiply the forward vector by the speed we set
            vFwdVec *= m_NoclipSpeed;

            //Get the side vector from the rotation (to move the player left and right)
            Vector3 vSideVec;
            vSideVec.X = 1 - 2 * (m_NoclipTransform.Rotation.Y * m_NoclipTransform.Rotation.Y + m_NoclipTransform.Rotation.Z * m_NoclipTransform.Rotation.Z);
            vSideVec.Y = 2 * (m_NoclipTransform.Rotation.X * m_NoclipTransform.Rotation.Z + m_NoclipTransform.Rotation.W * m_NoclipTransform.Rotation.Z);
            vSideVec.Z = 2 * (m_NoclipTransform.Rotation.X * m_NoclipTransform.Rotation.Z - m_NoclipTransform.Rotation.W * m_NoclipTransform.Rotation.Y);

            // multiply the side vector by the input (to determine whether to move left or right)
            vSideVec *= vInput.Y;
            // multiply the side vector by the speed we set
            vSideVec *= m_NoclipSpeed;

            // if the reduce speed key is down, reduce the movement that will be applied
            if (m_ReduceSpeed)
            {
                vFwdVec *= 0.25f;
                vSideVec *= 0.25f;
            }

            // if the increase speed is down, multiply the movement that will be applied
            if (m_IncreaseSpeed)
            {
                vFwdVec *= 2.5f;
                vSideVec *= 2.5f;
            }

            m_NoclipExitSpeed = vFwdVec + vSideVec;

            // add the forward and side vectors to the final transform
            m_NoclipTransform.Position += vFwdVec;
            m_NoclipTransform.Position += vSideVec;

            // apply the transform to the camera
            Player.SetCameraTransform(m_NoclipTransform);
        }
    }

    /*
     * always instantiate and keep around the callback instances
     * otherwise the GC is going to collect them
     */
    private static dBindCallback ToggleNoclipCallbackFunc = new dBindCallback(ToggleNoclip);
    private static dBindCallback ToggleGodCallbackFunc = new dBindCallback(ToggleGod);
    private static dAdvancedBindCallback ReducePlayerSpeedCallbackFunc = new dAdvancedBindCallback(UpdateReduceSpeed);
    private static dAdvancedBindCallback IncreasePlayerSpeedCallbackFunc = new dAdvancedBindCallback(UpdateIncreaseSpeed);

    private static dCallback PostPlayerUpdateCallbackFunc = new dCallback(OnPostPlayerUpdate);

    /*
     * function that initializes our mod
     * it must always be a public static void function, otherwise the mod won't be loaded
     * it can be named whatever you want though, as long as you define it on the mod's info.json
     */
    public static void Init()
    {
        /*
         * while registering binds and callbacks, it's very important to pass an instances of dBindCallback/dCallback/dAdvancedCallback that are
         * kept in the scope / wont be garbage collected, otherwise it'll lead to errors
         */
        CBind NoclipBind = new CBind(EKeyCode.VK_N, ToggleNoclipCallbackFunc);
        CBind GodBind = new CBind(EKeyCode.VK_INSERT, ToggleGodCallbackFunc);

        CBind ReduceSpeedBind = new CBind(EKeyCode.VK_CONTROL, ReducePlayerSpeedCallbackFunc);
        CBind IncreaseSpeedBind = new CBind(EKeyCode.VK_SHIFT, IncreasePlayerSpeedCallbackFunc);

        CCallback PostPlayerUpdateCallback = new CCallback(ECallbackType.PostPlayerUpdate, PostPlayerUpdateCallbackFunc);

        Log.General("examplemod successfully loaded");
        Log.General("controls: \n\t-insert: toggle godmode\n\t-n: toggle noclip\n\t-ctrl: slow down noclip\n\t-shift: speed up noclip");
    }
}