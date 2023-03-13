# Changelog

## [0.0.2] - 2023-03-13

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