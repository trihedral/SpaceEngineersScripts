// DECOMPRESSION DOORS CONTROL SCRIPT //
////////////////////////////////////////

const int delayTime = 30;

const int CLOSED = 0;
const int OPENING = 1;
const int SEALING = 2;
const int CLOSING = 3;

int state, stateTime;
IMyDoor enterDoor, exitDoor;

public Program () {
    Runtime.UpdateFrequency = UpdateFrequency.Update1;  // run every 1 tick
    state = CLOSED;
}

public void Main (string argument, UpdateType updateSource) {
    var outerDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("Outer Door");
    var innerDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("Inner Door");

    if (IsClosed(innerDoor)) outerDoor.ApplyAction("OnOff_On");
    else outerDoor.ApplyAction("OnOff_Off");
    if (IsClosed(outerDoor)) innerDoor.ApplyAction("OnOff_On");
    else innerDoor.ApplyAction("OnOff_Off");
    
    if (state == CLOSED && (IsOpening(innerDoor) || IsOpening(outerDoor)) ) {
        if ( IsOpening(innerDoor) ) {
            enterDoor = innerDoor;
            exitDoor = outerDoor;
        }
        else if ( IsOpening(outerDoor) ) {  
            enterDoor = outerDoor;
            exitDoor = innerDoor;
        }
        stateTime = 0;
        state = OPENING;
    }

    if (state == OPENING && stateTime >= delayTime) { 
        enterDoor.ApplyAction("Open_Off");
        stateTime = 0;
        state = SEALING;
    }

    if (state == SEALING) {
        if (IsClosed(enterDoor)) {
            exitDoor.ApplyAction("Open_On");
            stateTime = 0;
            state = CLOSING;
        }   
        enterDoor.ApplyAction("Open_Off");
    }
            

    if (state == CLOSING && stateTime >= delayTime) {
        exitDoor.ApplyAction("Open_Off");
        stateTime = 0;
        state = CLOSED;
    }

    Echo (outerDoor.Status.ToString());
    Echo (innerDoor.Status.ToString());

    stateTime++;
}

bool IsOpen (IMyDoor door) {
    return door.Status.ToString() == "Open";
}

bool IsClosed (IMyDoor door) {
    return door.Status.ToString() == "Closed";
}

bool IsOpening (IMyDoor door) {
    return door.Status.ToString() == "Opening";
}

bool IsClosing (IMyDoor door) {
    return door.Status.ToString() == "Closing";
}
