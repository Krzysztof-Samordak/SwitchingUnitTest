from scan_os.connect import find_and_connect_sync

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
    
print('Script Completed')
exit()