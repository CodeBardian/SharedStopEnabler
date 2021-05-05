## Changelog

### [1.1.4] - 2021-05-05

#### Changed
- removed unnecessary parts of codebase
- use harmony transpilers instead of bulk copying original code to postfix
#### Fixed
- wrong stop props appearing on removing transport lines from a shared stop
- exception on scanning subscribed mods ([#14](https://github.com/CodeBardian/SharedStopEnabler/issues/14))

### [1.1.3] - 2021-02-07

#### Changed
- bump CitiesHarmony to 2.0.4-2

### [1.1.2] - 2021-02-07

#### Changed
- upgrade harmony to 2.0.4
#### Fixed
- incompatibility with advanced stop selection mod. Thanks @Teach via steam.

### [1.1.1] - 2020-12-31

#### Fixed
- exception on mod startup caused by custom road assets without proper lane config ([#13](https://github.com/CodeBardian/SharedStopEnabler/issues/13)). Thanks @GameBurrow.

### [1.1.0] - 2020-08-23

#### Added
- load old shared stops at startup
- copy shared stops on updating segments
#### Fixed
- wrong stop layouts on moving stops ([#10](https://github.com/CodeBardian/SharedStopEnabler/issues/10))
- several minor fixes regarding removal of stops and transport lines


### [1.0.4] - 2020-07-14

#### Fixed
- wrong pathfinding on shared stops ([#1](https://github.com/CodeBardian/SharedStopEnabler/issues/1))
- incompatibility with advanced stop selection mod

### [1.0.3] - 2020-06-22

#### Added
- elevated (shared) stops
#### Fixed
- passengers waiting at wrong location ([#5](https://github.com/CodeBardian/SharedStopEnabler/issues/5))

### [1.0.2] - 2020-05-26

#### Changed
- migrate to harmony 2.0.0.9
#### Fixed
- wrong behaviour of platforms
- missing road meshes ([#7](https://github.com/CodeBardian/SharedStopEnabler/issues/7))
- wrong prop appearance on shared stops ([#2](https://github.com/CodeBardian/SharedStopEnabler/issues/2))
- walking tours won't complete ([#4](https://github.com/CodeBardian/SharedStopEnabler/issues/4))

### [1.0.1] - 2020-05-14
- first public release
