from scan_os.connect import find_and_connect_sync
import time
import sys
import msvcrt
import math
from threading import Thread

ROTATION1_MOTOR = 1
ROTATION2_MOTOR = 2
SWITCHING_UNIT_MOTOR = 3
NUMBER_OF_TEST = 5
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
scanner = find_and_connect_sync("1FD2227008X")
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
input()
stop_threads = True
print('Test-01-Rotation1Motor-select', flush=True)

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
input()
stop_threads = True
print('Test-03-Rotation2Motor-select', flush=True)

# Rotation2 - switch test
time.sleep(1)
rotation1Homed = False
rotation1Homed = rotation2.home().wait()
print('Test-04-Rotation2Switch-' + str(rotation2.homed), flush=True)

switchingUnit = services['switching_unit']

# SwitchingUnit - switch test
time.sleep(1)
switchingUnitHomed = False
switchingUnitHomed = switchingUnit.home().wait()
print('Test-05-SwitchingUnitSwitch-' + str(switchingUnit.homed), flush=True)
print('Script Completed')
exit()