using System.Numerics;

public class examplemod
{
    static bool m_GodModeEnabled = false;
    static bool m_NoclipEnabled = false;

    static bool m_ReduceSpeed = false;
    static bool m_IncreaseSpeed = false;

    static Vector3 vPlayerPos = new Vector3(.0f, .0f, .0f);

    static Vector3 vNoclipExitSpeed = new Vector3(.0f, .0f, .0f);
    static Vector3 vEmptyV3 = new Vector3(.0f, .0f, .0f);

    static Transform tNoclipTransform = new Transform();

    static float fNoclipSpeed = .25f;

    // function that gets called every time the god mode key is pressed
    static void ToggleGodMode()
    {
        m_GodModeEnabled = !m_GodModeEnabled;
        if (m_GodModeEnabled)
            SledgeLib.WriteLog("God mode enabled");
        else
            SledgeLib.WriteLog("God mode disabled");
    }

    // function that gets called every time the noclip key is pressed
    static void ToggleNoclip()
    {
        m_NoclipEnabled = !m_NoclipEnabled;
        if (m_NoclipEnabled)
        {
            // store the original camera transform
            tNoclipTransform = SledgeLib.Player.m_CameraTransform;
            SledgeLib.WriteLog("Noclip enabled");
        }
        else
        {
            // apply the exit speed to the player
            SledgeLib.Player.m_Velocity = vNoclipExitSpeed * 50;
            SledgeLib.WriteLog("Noclip disabled");
        }
    }

    // function that gets called every time the reduce speed key is pressed **or** released, and while the key is held
    static void SetReduceSpeed(bool bKeyDown)
    {
        m_ReduceSpeed = bKeyDown;
    }

    // function that gets called every time the increase speed key is pressed **or** released, and while the key is held
    static void SetIncreaseSpeed(bool bKeyDown)
    {
        m_IncreaseSpeed = bKeyDown;
    }

    // Function that gets called right before the game updates
    static void OnPreUpdate()
    {
        // if god mode is enabled, constantly set the player's health to 1
        if (m_GodModeEnabled)
            SledgeLib.Player.m_Health = 1;

        if (m_NoclipEnabled)
        {
            // Set the player's velocity to 0 (to avoid taking fall damage)
            SledgeLib.Player.m_Velocity = vEmptyV3;

            // store the camera rotation, since it'll be re-applied to the camera transform later
            tNoclipTransform.Rotation = SledgeLib.Player.m_CameraTransform.Rotation;

            // Read the input keys (the values change based on what key is pressed, W and S control the X axis, A and D control the Y axis)
            Vector2 vInputKeys = SledgeLib.Player.m_MovementKeys;

            // Get the forward vector from the rotation (to move the player forwards and backwards)
            Vector3 vFwdVec;
            vFwdVec.X = 2 * (tNoclipTransform.Rotation.X * tNoclipTransform.Rotation.Z + tNoclipTransform.Rotation.W * tNoclipTransform.Rotation.Y);
            vFwdVec.Y = 2 * (tNoclipTransform.Rotation.Y * tNoclipTransform.Rotation.Z - tNoclipTransform.Rotation.W * tNoclipTransform.Rotation.X);
            vFwdVec.Z = 1 - 2 * (tNoclipTransform.Rotation.X * tNoclipTransform.Rotation.X + tNoclipTransform.Rotation.Y * tNoclipTransform.Rotation.Y);
            
            // multiply the forward vector by the input (to determine whether to move forwards or backwards)
            vFwdVec *= -vInputKeys.X;
            // multiply the forward vector by the speed we set
            vFwdVec *= fNoclipSpeed;

            // Get the side vector from the rotation (to move the player left and right)
            Vector3 vSideVec;
            vSideVec.X = 1 - 2 * (tNoclipTransform.Rotation.Y * tNoclipTransform.Rotation.Y + tNoclipTransform.Rotation.Z * tNoclipTransform.Rotation.Z);
            vSideVec.Y = 2 * (tNoclipTransform.Rotation.X * tNoclipTransform.Rotation.Z + tNoclipTransform.Rotation.W * tNoclipTransform.Rotation.Z);
            vSideVec.Z = 2 * (tNoclipTransform.Rotation.X * tNoclipTransform.Rotation.Z - tNoclipTransform.Rotation.W * tNoclipTransform.Rotation.Y);

            // multiply the side vector by the input (to determine whether to move left or right)
            vSideVec *= vInputKeys.Y;
            // multiply the side vector by the speed we set
            vSideVec *= fNoclipSpeed;

            if (m_ReduceSpeed)
           {
                vFwdVec *= 0.25f;
                vSideVec *= 0.25f;
            }

            if (m_IncreaseSpeed)
            {
                vFwdVec *= 2.5f;
                vSideVec *= 2.5f;
            }

            // calculate the exit speed (for when the player disables noclip)
            vNoclipExitSpeed = vFwdVec + vSideVec;

            // add the forward and side vectors to the final transform
            tNoclipTransform.Position += vFwdVec;
            tNoclipTransform.Position += vSideVec;

            // set the player position to where our camera is (minus 1.7 which is the camera height)
            vPlayerPos = tNoclipTransform.Position;
            vPlayerPos.Y -= 1.7f;
            SledgeLib.Player.m_Position = vPlayerPos;

            // apply the transform to the camera
            SledgeLib.Player.m_CameraTransform = tNoclipTransform;
        }
    }

    // Instantiate and store the delegate instances
    // (so the GC doesn't collect them)
    private static dBindCallback ToggleGodCallback      =   new dBindCallback(ToggleGodMode);
    private static dBindCallback ToggleNoclipCallback   =   new dBindCallback(ToggleNoclip);
    private static dAdvancedBindCallback ReduceSpeedCallback = new dAdvancedBindCallback(SetReduceSpeed);
    private static dAdvancedBindCallback IncreaseSpeedCallback = new dAdvancedBindCallback(SetIncreaseSpeed);
    private static dCallback PreUpdateCallback          =   new dCallback(OnPreUpdate);

    public static void Init()
    {
        // register the normal binds
        // these binds only get called once the key is down
        CBind GodModeBind = new CBind(EKeyCode.VK_HOME, ToggleGodCallback);
        CBind NoclipBind = new CBind(EKeyCode.VK_N, ToggleNoclipCallback);

        // register the advanced binds
        // these binds get called when the key is pressed, when the key is released, and while it's held
        CAdvancedBind ReduceSpeedBind = new CAdvancedBind(EKeyCode.VK_CONTROL, ReduceSpeedCallback);
        CAdvancedBind IncreaseSpeedBind = new CAdvancedBind(EKeyCode.VK_SHIFT, IncreaseSpeedCallback);

        // register the callback
        CCallback PostCallback = new CCallback(ECallbackType.PostPlayerUpdate, PreUpdateCallback);

        SledgeLib.WriteLog("sledge example mod succesfully loaded");
        SledgeLib.WriteLog("controls: \n\t-n: toggle noclip\n\t-insert: toggle godmode\n\t-ctrl: slow down noclip\n\t-shift: speed up noclip");
    }
}