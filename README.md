> Automation is driving the decline of banal and repetitive tasks ~Amber Rudd

# TDMController
**TDMController** is an application for controlling a measurement system. It runs exclusively on Windows 10/11 and is not compatible with other operating systems.
The software allows for:
+ **Stepper motor control**: Supports models like [28BYJ-48](https://botland.store/stepper-motors/12807-stepper-motor-28byj-48-5v-01a-003nm-with-uln2003-controller-5904422306410.html)
+ **Optical line control**: Supports both custom optical lines (based on NEMA stepper motors) named PODL and ODL products from [ThorLabs](https://www.thorlabs.com/thorproduct.cfm?partnumber=ODL300/M)
+ **Power meter connection**: Enables continuous display of measured values using the [Thorlabs PM100D](https://www.thorlabs.com/newgrouppage9.cfm?objectgroup_id=3341)
+ **Trigger signal transmission**: Facilitates communication with devices such as a photon counter or a CCD camera
+ **Automated measurement series execution**: user-defined sequences of actions, where measurements are taken at specific steps within the sequence. This enables precise, repeatable, and automated data collection for experiments or calibration processes.

From a technical perspective, the fundamental operational unit of the system is a branch. A branch consists of an Arduino board connected to 28BYJ-48 stepper motors and custom-designed optical lines. The custom optical lines can be freely replaced with ThorLabs lines (which, however, are connected separately via the [KCube](https://www.thorlabs.com/newgrouppage9.cfm?objectgroup_id=2424) controller).

Additionally, each Arduino board can be connected to an external device, allowing it to send a trigger signal.
The Arduino connection diagram for the system is available in the **BranchProject** directory. This folder also contains the Arduino code required for controlling the stepper motors and optical lines.

# Implementation
The application is written in `.NET 8.0` using [**AvaloniaUI**](https://avaloniaui.net/). Its structure follows the MVVM pattern, based on a local data structure:

+ The `ViewModel` layer communicates with the View through two-way binding (using INotifyPropertyChanged from the [MVVM Toolkit package](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/).
+ The `Model` layer utilizes services and [dependency injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) (DI Container): services provide the necessary data directly to the constructors of `ViewModel` objects.

Structured variables in the program, such as measurement system projects, recorded series, and settings, are stored as `.json` files.

## View and ViewModel Structures
The `View` and `ViewModel` layers consist of three main modules, corresponding to the three primary tasks of the software:
+ **TDM**: Handles manual system control.
+ **Series**: Manages and executes measurement series.
+ **Projects**: Loads measurement system projects.

The TDM module dynamically adjusts the view to the screen size. The current version of the program includes three possible display modes, corresponding to the following views: `TDMFullyCollapsedView`, `TDMCollapsedView`, and `TDMExpandedView`. The choice of which content to display is based on the available width.

All Views corresponding to this module share the same ViewModel (as they display the same information, just in different formats). Below is an example screenshot of the `TDMExpandedView`:
<center>
  <img src="https://github.com/user-attachments/assets/b5dc116b-8269-4216-aa37-3eb8958221a2">
</center>

In the Series module, there are two views – one for loading a series and another for monitoring the status of a running series. The active series view features a progress indicator in the form of a progress bar and displays logs related to performed actions and measurements.

Below is a screenshot after loading a series file:
<center>
  <img src="https://github.com/user-attachments/assets/882f0560-e559-48bc-8e3a-8dae08ddd89b">
</center>

The last opened project is automatically saved and loaded upon the next program launch. The Projects and Series modules currently support only loading `.json` files. Creating custom schemes within the program is not yet supported.

Example files can be found in the repository in the following directories:
+ _UserProjects_: project files for the measurement system
+ _UserSeries_: measurement series files

## Model Structure
The primary representation of the measurement system in the program code is the `Project` class. This class contains a list of objects representing the system's branches and a class for handling communication with the power meter.
A simplified diagram of the module responsible for communication with the measurement system is shown below:

<div align="center">
  <picture>
    <source srcset="https://github.com/user-attachments/assets/90915a32-e556-4c94-b7b8-c4fad3d6d74d" media="(prefers-color-scheme: light)">
    <source srcset="https://github.com/user-attachments/assets/99afb55e-3c3f-460b-ab09-9c206cebe35d" media="(prefers-color-scheme: dark)">
    <img src="https://github.com/user/repo/assets/123456789/light-mode-image.png" alt="Dynamic image">
  </picture>
</div>

The `Model` layer includes a service module that enables dependency injection for `ViewModel` objects. Two services have been implemented in the project:

+ **ProjectService**: Passes the currently used project to the ViewModel layer.
+ **LastProjectService**: Loads the last opened project when the application starts.

### Controlling ThorLabs Devices:
Integration with ThorLabs devices is achieved using the manufacturer's provided [API](https://www.thorlabs.com/). External libraries are included as references and used in the `TLPositionDevice` and `TLPowerMeter` classes.

## Example Usage
The program was used to split a single laser pulse into four separate pulses. The system allows for modulation of power and temporal separation of the split beams. The diagram of the described system is shown below.
<div align="center">
  <picture>
    <source srcset="https://github.com/user-attachments/assets/99e53958-966e-4d4e-bf6c-28aaa22a8b4f" media="(prefers-color-scheme: light)">
    <source srcset="https://github.com/user-attachments/assets/a2a2078b-971b-4989-b870-5e1403e9f4a0" media="(prefers-color-scheme: dark)">
    <img src="https://github.com/user/repo/assets/123456789/light-mode-image.png" alt="Dynamic image">
  </picture>
</div>
