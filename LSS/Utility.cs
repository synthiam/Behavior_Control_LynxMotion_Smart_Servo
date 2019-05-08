using static EZ_B.Servo;

namespace LSS {

  public class Utility {

    public static byte GetIdFromServo(ServoPortEnum servo) {

      return (byte)(servo - EZ_B.Servo.ServoPortEnum.V0);
    }
  }
}
