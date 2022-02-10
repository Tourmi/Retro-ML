# Super Mario World - Machine Learning
A project by Tourmi & Voltage98 
--------
Training a Super Mario World AI that's able to beat levels on its own, as well as completing or optimizing various objectives  
[License](LICENSE)

## When cloning
It is important to either clone the repository recursively to include the submodules, or initialize the submodules after cloning, as they need to be built for the program compilation to actually work.

## Prerequisites
* Must be on Windows
* .NET SDK 6.0 or higher must be installed

## Building the dependencies
Run these commands starting from the root of the repository

### SharpNEAT
```
cd .\Submodules\SharpNEAT\src\
dotnet build --configuration Release
```

### BizHawk
```
cd .\Submodules\BizHawk\Dist\
.\QuickTestBuildAndPackage.bat
```

## Building the application
```
cd .\SMW-ML\
dotnet build --configuration Release
```

## Running the application
To run the application, you first need to copy a Super Mario World rom file to the root directory of the program. It must be named "swm.sfc" exactly.

## Using the application

### Main page

#### ![Image of the main page of the application](docs/mainApp.png)
* [Start training](#training-page)
* Load population
  * Loads an existing population (xyz.pop) into the program. Allows to continue training a population after closing and reopening the application.
* Save population
  * Saves the population that's been trained for at least one generation.
* [Training Configuration](#configuration)

### Training Page

#### ![Image of the training page](docs/training-training.png)
When entering this page, the training of AIs will be started automatically, using the [app's configuration](#configuration). Note that the UI might be unresponsive if too many emulator instances are running at once. Please do not close any emulators manually, as this will break the application.
* Neural Network visualization
  * Shows the first emulator's neural network structure and values. When the emulators finish booting, the emulator being represented will be the one on the bottom. The refresh rate of the preview will depend on the computer's performances, and having too many emulators running at once will affect it.
* Stop Training
  * Will stop training at the end of the current generation. Please be patient if the population size is big, and not many emulators are running at once. It will return to the [main page](#main-page) once it is done.

### Configuration

#### ![Image of the neural network configuration menu](docs/config-neural.png)
* Number of AIs
  * Determines the total population size of the training. Making it too big will make evolution really slow, while making it too small will make break-throughs extremely rare.
* Species count
  * Determines the number of species to use for the NEAT algorithm. A higher value will make breakthroughs more common while training, but a value that's too high will be detrimental to the evolution of the individual species. The amount of AIs per species is equal to `Number of AI / Species Count`
* Elitism proportion
  * The percentage of species to keep in each generation. Should be higher than 0, but lower than 1. New species will be created from the species that are kept, either by sexual reproduction, or asexual reproduction
* Selection proportion
  * The percentage of AIs to keep between each generation, within a species. Should be higher than 0, but lower or equal to 1. New AIs will be created within the species based on the AIs that are kept.

#### ![Image of the app configuration menu](docs/config-app.png)
* Multithread
  * This is the amount of emulators which will be booted while training. It is recommended to not put this value higher than the amount of cores within your computer, as performance will be greatly affected. For the fastest training, at the cost of using up all of the CPU resources available, set to the exact amount of cores in your computer. Otherwise, set to a lower value.
* Communication Port with Arduino
  * Communication port with an Arduino that's connected to the PC. Should be left like it is if no arduinos are connected. Used so we can preview the inputs on an actual physical controller.
  * [See ./ArduinoSNESController](ArduinoSNESController)
