from scan_os.connect import find_and_connect_sync
import time
import sys
import msvcrt
import math
from threading import Thread

ROTATION1_MOTOR = 1
ROTATION2_MOTOR = 2
SWITCHING_UNIT_MOTOR = 3
NUMBER_OF_TEST = 8

def threaded_function(stop, number):
    if number == ROTATION1_MOTOR:
        motor = rotation1
    elif number == ROTATION2_MOTOR:
        motor = rotation2
    while True:
        motor.move(-2*math.pi, {}).wait()
        if stop():
            break 
          
stop_threads = False 
thread = Thread(target = threaded_function, args =(lambda : stop_threads, ROTATION1_MOTOR, ),)

print("Provide tester serial number!")
testerSerialNumber = input()
scanner = find_and_connect_sync(testerSerialNumber)
services = scanner.get_services()
services.keys()
device = services['device']
codeName = ''
codeName = device.code_name

if codeName != 'Blackstar' :
    print('Cannot connect', flush=True)
    exit(0)
else:
    print('Connected', flush=True)
  
rotation1 = services['rotation1']
input()
print('Number of tests -' +str(NUMBER_OF_TEST))

# Rotation1 - motor test
print('Check if Rotation1 is moving right', flush=True)
thread.start()
rotation1MotorResult = input()
stop_threads = True
print('Test-01-Rotation1Motor-' + rotation1MotorResult, flush=True)

# Rotation1 - switch test
time.sleep(1)
rotation1Homed = False
rotation1Homed = rotation1.home().wait()
print('Test-02-Rotation1Switch-' + str(rotation1.homed), flush=True)

rotation2 = services['rotation2']
thread = Thread(target = threaded_function, args =(lambda : stop_threads, ROTATION2_MOTOR, ),)

# Rotation2 - motor test
stop_threads = False
print("Check if Rotation2 is moving right", flush=True)
thread.start()
rotation2MotorResult = input()
stop_threads = True
print('Test-03-Rotation2Motor-' + rotation2MotorResult, flush=True)

# Rotation2 - switch test
time.sleep(1)
rotation1Homed = False
rotation1Homed = rotation2.home().wait()
print('Test-04-Rotation2Switch-' + str(rotation2.homed), flush=True)

switchingUnit = services['switching_unit']
endstop1Switch = services['endstop1_switch']
endstop2Switch = services['endstop2_switch']
switchHome = services['switching_unit_switch']

# SwitchingUnit - Switch Home test preparation
switch_home_first_position = False
switch_home_second_position = True

# SwitchingUnit - endstop1_switch. test
endstop1_switch = False
switchingUnit.high_impedance()
print("Rotate the SwitchingUnit right!", flush=True)
input()
endstop1_switch = endstop1Switch.triggered
switch_home_first_position = switchHome.triggered
print('Test-05-Endstop1_switch-' + str(endstop1_switch), flush=True)



# SwitchingUnit - endstop2_switch. test
endstop2_switch = False
switchingUnit.high_impedance()
print("Rotate the SwitchingUnit left!", flush=True)
input()
endstop2_switch = endstop2Switch.triggered
switch_home_second_position = switchHome.triggered
print('Test-06-Endstop2_switch-' + str(endstop2_switch), flush=True)

# SwitchingUnit - Switch Home test implementation
if switch_home_first_position == True and switch_home_second_position == False :
    switch_home_result = True
else:
    switch_home_result = False
print('Test-07-SwitchHome-' + str(switch_home_result), flush=True)

# SwitchingUnit - Motor Test
switchingUnitMotorTestResult = False;
#Firstly check if all of sensors are working properly
if switch_home_first_position == True and switch_home_second_position == False and endstop1_switch == True and endstop2_switch == True:
    #Move motor and check whatever endstop2Switch has been activeted, if not, the rotation direction is correct
    switchingUnit.move(-0.03*math.pi, {}).wait()
    endstop2_switch = endstop2Switch.triggered
    if endstop2_switch == False:
        print('Check if Switching Unit is Rotating Right!', flush=True)
        input()
        switchingUnit.move(-0.8*math.pi, {}).wait()
        print("Has Switching Unit rotated right?", flush=True)
        switchingUnitMotorTestResult = input()     
    else:
        print('EndStop2 activated! The SwitchingUnitMotor probably is rotating in wrong direction!')
else:
    print('Cannot perform SwitchingUnitMotor because of defective sensors!')
  
print('Test-08-SwitchingUnitMotor-' + str(switchingUnitMotorTestResult), flush=True)
    
print('Script Completed')
exit()