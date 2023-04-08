# Changelog

## [0.2.2] - 2023-04-08

### Fixed
- Fixed issue with incorrect variable used to dispose event data

## [0.2.1] - 2023-04-08

### Added
- Event data that implements IDisposable is now automatically disposed after event is raised

## [0.1.2] - 2023-03-19

### Fixed
- Unregistering a callback from an event callback no longer causes crashes.

## [0.1.1] - 2023-03-18

### Added
- It is now possible to raise an event only for callbacks with specified key.

## [0.0.3] - 2023-03-15

### Fixed
- Added assembly definition so that the package can be properly used

## [0.0.2] - 2023-03-13

### Added
- Callbacks are now called within a try-catch. If a callback crashes other callbacks will still be executed.
- More comprehensive logging for callback crashes.

### Fixed
- Callbacks can't be registered multiple times anymore. This doesn't apply to lambda expression callbacks.

### Changed
- Instead of throwing an expression, an error log is printed when trying to (un)register or raise an event with data that doesn't have a channel
- Keys for keyed event channels now have to be IEquatable.

## [0.0.1] - 2023-03-13

### Added
- EventSystem through which events can be registered
- EventChannels that are used by the system to pass data from provider to observer
- Supoort for Scriptable Object EventChannels