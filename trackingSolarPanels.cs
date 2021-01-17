const float spd = 0.05f;
const float delPwr = 5;

const int HOMING = 0;
const int WAITING = 1;
const int TRACKY = 2;
const int TRACKX = 3;
int state, xDir, yDir;
float pwr0;

public Program () {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;  // run every 100 ticks (every couple sec)
    state = WAITING;
    yDir = 1;
    xDir = 1;
}

public void Main (string argument, UpdateType updateSource) {
    var h1 = (IMyMotorAdvancedStator)GridTerminalSystem.GetBlockWithName("Hinge 1");
    var h2 = (IMyMotorAdvancedStator)GridTerminalSystem.GetBlockWithName("Hinge 2");
    List<IMyTerminalBlock> panels = new List<IMyTerminalBlock>();
    GridTerminalSystem.SearchBlocksOfName("Solar Panel", panels);
    IMyInteriorLight light =  (IMyInteriorLight)GridTerminalSystem.GetBlockWithName("Panel Light");

    float pwr = 0; 
    foreach (IMySolarPanel panel in panels) {
        pwr += GetPanelPower(panel);
    }

    if (pwr < 0.01) state = HOMING;
    if (state == HOMING) Home(h1, h2, light);
    if (state == WAITING) Wait(h2, pwr, light);
    if (state == TRACKY || state == TRACKX) Track(h1, h2, pwr, light);
    
    Echo ("Pitch: " + (h1.Angle).ToString());
    Echo ("Yaw: " + (h2.Angle).ToString());
    Echo ("Power: " + pwr.ToString());
    Echo ("");
    Echo (GetState());
    if (state == WAITING) {
        Echo ("Power change: " + Math.Abs(pwr - pwr0).ToString());
        Echo ("Power change threshold: " + delPwr.ToString());
    }

    if (state != WAITING) pwr0 = pwr;
}

public float GetPanelPower (IMySolarPanel panel) { 
	    string[] detail = (panel.DetailedInfo.Split('\n',' '));
	    return Convert.ToSingle(detail[5]);
} 

void Home (IMyMotorAdvancedStator h1, IMyMotorAdvancedStator h2, IMyInteriorLight light) {
    light.SetValue<Color>( "Color",  new Color(255, 0, 0) ); 
    if (h1.Angle < 1) h1.TargetVelocityRPM = 1;
    else h1.TargetVelocityRPM = 0;
    if (h2.Angle > -1.2) h2.TargetVelocityRPM = -1;
    else h2.TargetVelocityRPM = 0;
    if ( h1.TargetVelocityRPM == 0 &&  h2.TargetVelocityRPM == 0) state = WAITING;
}

void Wait (IMyMotorAdvancedStator h2, float pwr, IMyInteriorLight light) {
    light.SetValue<Color>( "Color",  new Color(255, 255, 0) ); 
    if (Math.Abs(pwr0 - pwr) > delPwr) {
        state = TRACKX;
        h2.TargetVelocityRPM = spd*xDir;
        xDir = - xDir;
        pwr0 = pwr;
    }
}

void Track (IMyMotorAdvancedStator h1, IMyMotorAdvancedStator h2, float pwr, IMyInteriorLight light) {
    light.SetValue<Color>( "Color",  new Color(0, 255, 0) ); 
    if (pwr < pwr0) { 
        if (state == TRACKX) {
            h2.TargetVelocityRPM = 0;
            state = TRACKY;
            h1.TargetVelocityRPM = -spd*yDir;
            yDir = -yDir;
        }
        else if (state == TRACKY) {
            h1.TargetVelocityRPM = 0;
            state = WAITING;
        }
    }
}

string GetState(){
    string stateStr = "Homing";
    if (state == WAITING) stateStr = "Waiting";
    else if (state == TRACKX) stateStr = "Tracking X";
    else if (state == TRACKY) stateStr = "Tracking Y";
    return stateStr;
}
