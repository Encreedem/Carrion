The Carrion Manager Console (CMC) assists you with installing, uninstalling and launching custom maps for Phobia's game Carrion.
Additionally it will automatically backup and load your save files to prevent them from being overwritten when you play different levels.

For a detailed explanation on how to get started and how to use CMC, check out the GitHub Wiki:
https://github.com/Encreedem/Carrion/wiki

### Prerequisites ###
The game Carrion by Phobia Game Stuido
A 64 bit operating system
.Net Core 3.1

### Getting Started ###
If you haven't already, download the newest release of the Carrion Manager Console and extract it anywhere on your PC
Open the extracted folder and edit the configuration file "CarrionManagerConsole.cfg".
    If the file doesn't exist then start CarrionManagerConsole.exe to automatically create it.

### Config File CarrionManagerConsole.cfg ###
Settings are saved in this format: key=value
Quotation marks before and after the value but not within the value will be ingored.
Spaces before and after the equals sign will be ignored.
Capitalization for e.g. "Steam", "Directly", "true" or "false" doesn't matter.

## Settings ##
LaunchMethod:   If Carrion is installed via steam, enter "Steam", otherwise "Directly".
SteamPath:      The full path to steam.exe
GamePath:       The full path to the folder containing carrion.exe (i.e. where Carrion is installed)
BackupsPath:    The full path to the folder where saves and files will be backed up.
CustomMapsPath: The full path to the folder containing all your extracted custom maps
AppDataPath:    The full path to the folder containing Carrion's saves folder and settings.json
MappingTools:   Whether to display the Mapping Tools window (only needed by mappers).
ManageSaves:    Whether saves should automatically be backed up and loaded when you launch a map.

### Download and Install Custom Maps ###
(optional) Create a Custom Maps folder and write its path into CarrionManagerConsole.cfg
Download one or more custom maps
Extract the map(s) to the Custom Maps folder
Install the map via the CMC Map Installer

The maps in the Custom Maps folder should have the following structure:
Custom Maps
- Hideout
    - Levels
        - hideout.json
    - Scripts
        - hideout.cgs

You can also put maps in subfolders. E.g.:
Custom Maps
- Encreedem
    - Hideout
        - Levels
            - hideout.json
        - Scripts
            - hideout.cgs

### Feedback ###
If you have any questions, suggestions or feedback related to the Carrion Manager Console, you can message Encreedem in the CARRION Modding Discord -> #level-loader-project or via DM in Discord to Encreedem#0001.
If you found a bug, report it via one of the above mentioned methods or by opening an issue via GitHub: https://github.com/Encreedem/Carrion/issues

### License ###
This project is licensed under the MIT License - see the LICENSE.txt file for details.