UNITY_BUILD_DIR="$(pwd)/build"
UNITY_PROJECT_PATH="$(pwd)/UnityTest/UnityTest"
LOG_DIR="$(pwd)/log"
LOG_FILE="$LOG_DIR/unity-build.log"

# Build the test project for Windows
# See https://docs.unity3d.com/Manual/CommandLineArguments.html for command line arguments
rvmsudo mkdir "$UNITY_BUILD_DIR"
rvmsudo mkdir "$LOG_DIR"
rvmsudo u3d run -- \
  -batchmode \
  -buildWindows64Player "$UNITY_BUILD_DIR/win64/binokel-deluxe.exe" \
  -nographics \
  -projectPath "$UNITY_PROJECT_PATH" \
  -quit \
  -silent-crashes \
  -logFile "$LOG_DIR/unity-build.log"

# Now build for android
#"/c/program files/unity/editor/unity.exe" \
#  -batchmode \
#  -buildtarget Android \
#  -nographics \
#  -projectPath "$UNITY_PROJECT_PATH" \
#  -quit \
#  -silent-crashed
  

# Print the log file
cat $LOG_FILE