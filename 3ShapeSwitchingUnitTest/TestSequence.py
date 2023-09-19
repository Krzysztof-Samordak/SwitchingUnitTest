from scan_os.connect import find_and_connect_sync
import time
import sys
import msvcrt
import math
from threading import Thread

# auxilary variables
ROTATION1_MOTOR = 1
ROTATION2_MOTOR = 2
SWITCHING_UNIT_MOTOR = 3
NUMBER_OF_TEST = 9
# auxilary variables

# Test limits
INDUCTIVE_SWITCH_GAP_TEST_POSITIONS_CORRECT_VALUE 2
# Test limits

def threaded_function(stop, number):
    if number == ROTATION1_MOTOR:
        motor = rotation1
    elif number == ROTATION2_MOTOR:
        motor = rotation2
    while True:
        motor.move(-2*math.pi, {}).wait()
        if stop():
            break 
            
def measure_trigger_points(axis, sw):
    axis.home()
    axis.step_to(-10, {}) # Make sure we start with some distance to the home switch
    current = sw.is_triggered()
    points = []
    for i in range(360):
        axis.move_to(i * math.pi / 180, {'vibration_settle_time': 0.1}).wait()
        s = sw.is_triggered()     
        if s != current:
            points.append((axis.get_step_position(), s))
            current = s   
    return points
          
stop_threads = False 
thread = Thread(target = threaded_function, args =(lambda : stop_threads, ROTATION1_MOTOR, ),)

print("Provide tester serial number!")
testerSerialNumber = input()

try:
    scanner = find_and_connect_sync(testerSerialNumber)
    time.sleep(1)
    scanner.reset()
    time.sleep(1)
    scanner = find_and_connect_sync(testerSerialNumber)
    services = scanner.get_services()
    device = services['device']
    services.keys()
except:  
    print('TimeoutError', flush=True)
    exit(0)

print('Connected', flush=True)
input()
print('Number of tests -' +str(NUMBER_OF_TEST))

#  Articulator Plate Switch Test
try:
    articulatorPlateSwitch = services['articulator_switch']
except:  
    articulatorPlateSwitch = None
    articulatorPlateSwitchResult = None

articulator_switch_first_position = False
articulator_switch_second_position = True

if articulatorPlateSwitch is not None:

    articulatorPlateSwitchResult = False

    print('Place Articulator Plate on Rotation1 and press OK', flush=True)
    input()

    try:
        articulator_switch_first_position = articulatorPlateSwitch.triggered
        print(str(articulator_switch_first_position), flush=True)
        
    except:  
        None

    print('Remove Articulator Plate from Rotation1 and press OK', flush=True)
    input()

    try:
        articulator_switch_second_position = articulatorPlateSwitch.triggered
        print(str(articulator_switch_second_position), flush=True)
    except:  
        None

    if articulator_switch_first_position == True and articulator_switch_second_position == False:
        articulatorPlateSwitchResult = True
print('Test-01-ArticulatorPlateSwitch-' + str(articulatorPlateSwitchResult), flush=True)


# Rotation1 - motor test
rotation1MotorResult = False
try:
    rotation1 = services['rotation1']
except:  
    rotation1 = None
    rotation1MotorResult = None;
if rotation1 is not None:     
    print('Check if Rotation1 is moving right', flush=True)
    thread.start()
    rotation1MotorResult = input()
    stop_threads = True
print('Test-02-Rotation1Motor-' + str(rotation1MotorResult), flush=True)


# Rotation1 - switch test
time.sleep(1)
rotation1Homed = False
if rotation1MotorResult == 'True':
    try:
        rotation1.home().wait()
        rotation1Homed = rotation1.homed
    except:  
        print()    
else:
    print('Cannot perform Rotation1Switch test because of defective motor', flush=True)
    rotation1Homed = None
    input()
print('Test-03-Rotation1Switch-' + str(rotation1Homed), flush=True)

# Rotation1 - inductive switch gap test
time.sleep(1)
rotation1InductiveSwitchGapTestResult = False
rotation1InductiveSwitchGapTestValue = 0        #Number of detected positions in which there was sensor state change detected

try:
    rotation1Switch = services['rotation1_switch']
except:  
    rotation1Switch = None
    rotation1InductiveSwitchGapTestResult = None
    
if rotation1Switch is not None:
    try:
        points[] = measure_trigger_points(rotation1, rotation1Switch)
    except:  
        points[] = None 
        rotation1InductiveSwitchGapTestResult = None
        
if points is not None:
    rotation1InductiveSwitchGapTestValue = len(points)
    
if rotation1InductiveSwitchGapTestValue == INDUCTIVE_SWITCH_GAP_TEST_POSITIONS_CORRECT_VALUE
    rotation1InductiveSwitchGapTestResult = True
print('Test-04-Rotation1InductiveSwitchGapTest-' + str(rotation1InductiveSwitchGapTestResult), flush=True)

# Rotation2 - motor test
thread = Thread(target = threaded_function, args =(lambda : stop_threads, ROTATION2_MOTOR, ),)
rotation2MotorResult = False
try:
    rotation2 = services['rotation2']
except:  
    rotation2 = None
    rotation2MotorResult = None
if rotation2 is not None:
    stop_threads = False
    print("Check if Rotation2 is moving right", flush=True)
    thread.start()
    rotation2MotorResult = input()
    stop_threads = True
print('Test-05-Rotation2Motor-' + str(rotation2MotorResult), flush=True)

# Rotation2 - switch test
time.sleep(1)
rotation2Homed = False
if rotation2MotorResult == 'True':
    try:
        rotation2.home().wait()
        rotation2Homed = rotation2.homed
    except:  
        print()
else:
    print('Cannot perform Rotation2Switch test because of defective motor', flush=True)
    rotation2Homed = None
    input()
print('Test-06-Rotation2Switch-' + str(rotation2Homed), flush=True)

# Rotation2 - inductive switch gap test
time.sleep(1)
rotation2InductiveSwitchGapTestResult = False
rotation2InductiveSwitchGapTestValue = 0        #Number of detected positions in which there was sensor state change detected

try:
    rotation2Switch = services['rotation2_switch']
except:  
    rotation2Switch = None
    rotation2InductiveSwitchGapTestResult = None
    
if rotation2Switch is not None:
    try:
        points[] = measure_trigger_points(rotation2, rotation2Switch)
    except:  
        points[] = None 
        
if points is not None:
    rotation2InductiveSwitchGapTestValue = len(points)
    
if rotation2InductiveSwitchGapTestValue == INDUCTIVE_SWITCH_GAP_TEST_POSITIONS_CORRECT_VALUE
    rotation2InductiveSwitchGapTestResult = True
print('Test-07-Rotation2InductiveSwitchGapTest-' + str(rotation2InductiveSwitchGapTestResult), flush=True)

# SwitchingUnit -Tests preparation
try:
    switchingUnit = services['switching_unit']
except:  
    switchingUnit = None

try:
    endstop1Switch = services['endstop1_switch']
except:  
    endstop1Switch = None
    endstop1_switch = None

try:
    endstop2Switch = services['endstop2_switch']
except:  
    endstop2Switch = None
    endstop2_switch = None
    
try:
    switchHome = services['switching_unit_switch']
except:  
    switchHome = None
    switch_home_result = None

switch_home_first_position = False
switch_home_second_position = True

# SwitchingUnit - endstop1_switch. test
endstop1_switch_result = False

if endstop1Switch is not None: 
    endstop1_switch_active = False
    endstop1_switch_inactive = True
    try:
        switchingUnit.high_impedance()
    except:
        print()
    try:
        print("Rotate the SwitchingUnit right!", flush=True)
        input()
        endstop1_switch_active = endstop1Switch.triggered
        endstop2_switch_inactive = endstop2Switch.triggered
        switch_home_first_position = switchHome.triggered
    except:  
        print()


# SwitchingUnit - endstop2_switch. test
endstop2_switch_result = False

if endstop2Switch is not None: 
    endstop2_switch_active = False
    endstop2_switch_inactive = True
    try:
        switchingUnit.high_impedance()
    except:
        print()
    try:
        print("Rotate the SwitchingUnit left!", flush=True)
        input()
        endstop2_switch_active = endstop2Switch.triggered
        endstop1_switch_inactive = endstop1Switch.triggered
        switch_home_second_position = switchHome.triggered
    except:  
        print()
        
    if endstop1_switch_inactive == False and endstop1_switch_active == True: 
        endstop1_switch_result = True
    if endstop2_switch_inactive == False and endstop2_switch_active == True: 
        endstop2_switch_result = True    

print('Test-08-Endstop1_switch-' + str(endstop1_switch_result), flush=True)        
print('Test-09-Endstop2_switch-' + str(endstop2_switch_result), flush=True)

# SwitchingUnit - Switch Home test implementation
if switchHome is not None:
    if switch_home_first_position == True and switch_home_second_position == False :
        switch_home_result = True
    else:
        switch_home_result = False
print('Test-10-SwitchHome-' + str(switch_home_result), flush=True)

# SwitchingUnit - Motor Test
switchingUnitMotorTestResult = False;

#Firstly check if all of sensors are working properly
if switch_home_first_position == True and switch_home_second_position == False and endstop1_switch == True and endstop2_switch == True:
#Move motor and check whatever endstop2Switch has been activeted, if not, the rotation direction is correct
    try:
        switchingUnit.move(-0.03*math.pi, {}).wait()
    except:
        None
    else:
        endstop2_switch = endstop2Switch.triggered
        if endstop2_switch == False:
            print('Check if Switching Unit is Rotating Right!', flush=True)
            input()
            try:
                switchingUnit.move(-0.8*math.pi, {}).wait()
            except:
                None
            print("Has Switching Unit rotated right?", flush=True)
            switchingUnitMotorTestResult = input()     
        else:
            print('EndStop2 activated! The SwitchingUnitMotor probably is rotating in wrong direction!')
else:
    print('Cannot perform SwitchingUnitMotor because of defective sensors!')
    switchingUnitMotorTestResult = None
print('Test-11-SwitchingUnitMotor-' + str(switchingUnitMotorTestResult), flush=True)
   
try:
    switchingUnit.high_impedance()
except:
    None
print('Script Completed')
exit()