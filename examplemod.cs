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
    private static dBindCallback ToggleNoclip = new dBindCallback(() => {
        m_NoclipEnabled = !m_NoclipEnabled;
        Log.General("Noclip {0}", m_NoclipEnabled ? "enabled" : "disabled");

        if (m_NoclipEnabled)
        {
            m_NoclipTransform = Player.GetCameraTransform(); // store the camera's last transform
        }
        else
        {
            Player.SetPosition(m_NoclipTransform.Position - new Vector3(0, 1.7f, 0)); // set the player's position to where the camera's position last was (minus the camera offset)
            Player.SetVelocity(m_NoclipExitSpeed * 50.0f); // apply the exit velocity to the player
        }
    });

    private static dBindCallback ToggleGod = new dBindCallback(() => {
        m_GodEnabled = !m_GodEnabled;
        Log.General("God mode {0}", m_GodEnabled ? "enabled" : "disabled");
    });

    /*
     *  these functions will be called when the key they're bound to is pressed or released
     *  bKeyDown will be true if the key was pressed, or false if it was released
     *  (it'll also be called multiple times while it's pressed)
     */
    private static dAdvancedBindCallback ReduceSpeed = new dAdvancedBindCallback((bool bKeyDown) => { m_ReduceSpeed = bKeyDown; });
    private static dAdvancedBindCallback IncreaseSpeed = new dAdvancedBindCallback((bool bKeyDown) => { m_IncreaseSpeed = bKeyDown; });

    /*
     * this function will be called every time the player is done updating (camera position updates, health updates, etc)
     */
    private static dBindCallback OnPostPlayerUpdate = new dBindCallback(() =>
    {
        // if the user is in the menu / doing osmething else, don't run this
        if (!Game.IsPlaying())
            return;

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
    });

    /*
     * while registering binds and callbacks, it's very important to pass an instances of dBindCallback/dCallback/dAdvancedCallback that are
     * kept in the scope / wont be garbage collected, otherwise it'll lead to errors
     */
    private static CBind? NoclipBind;
    private static CBind? GodBind;

    private static CBind? ReduceSpeedBind;
    private static CBind? IncreaseSpeedBind;

    private static CCallback? PostPlayerUpdateCallback;

    /*
     * function that starts our mod
     * it must always be a public static void function, otherwise the mod won't be loaded
     * it can be named whatever you want though, as long as you define it on the mod's info.json
     * but it'll also detect the following names automatically:
     * "Init", "Start", "InitMod", "LoadMod", "StartMod", "ModInit" and "ModStart"
     */
    public static void Init()
    {
        NoclipBind = new CBind(EKeyCode.VK_N, ToggleNoclip);
        GodBind = new CBind(EKeyCode.VK_INSERT, ToggleGod);
        ReduceSpeedBind = new CBind(EKeyCode.VK_CONTROL, ReduceSpeed);
        IncreaseSpeedBind = new CBind(EKeyCode.VK_SHIFT, IncreaseSpeed);
        PostPlayerUpdateCallback = new CCallback(ECallbackType.PostPlayerUpdate, OnPostPlayerUpdate);

        Log.General("example mod successfully loaded");
        Log.General("controls: \n\t-insert: toggle godmode\n\t-n: toggle noclip\n\t-ctrl: slow down noclip\n\t-shift: speed up noclip");
    }

    /*
     * function that stops our mod
     * it must always be a public static void function, otherwise it'll error while unloading
    * it can be named whatever you want though, as long as you define it on the mod's info.json
     * but it'll also detect the following names automatically:
     * "Shutdown", "Stop", "Disable", "ShutdownMod", "StopMod", "DisableMod", "ModShutdown"
     */
    public static void Shutdown()
    {
        if (NoclipBind != null)
        {
            NoclipBind.Unregister();
            NoclipBind = null;
        }

        if (GodBind != null)
        {
            GodBind.Unregister();
            GodBind = null;
        }

        if (ReduceSpeedBind != null)
        {
            ReduceSpeedBind.Unregister();
            ReduceSpeedBind = null;
        }

        if (IncreaseSpeedBind != null)
        {
            IncreaseSpeedBind.Unregister();
            IncreaseSpeedBind = null;
        }

        if (PostPlayerUpdateCallback != null)
        {
            PostPlayerUpdateCallback.Unregister();
            PostPlayerUpdateCallback = null;
        }

        Log.General("example mod succesfully unloaded");
    }
}