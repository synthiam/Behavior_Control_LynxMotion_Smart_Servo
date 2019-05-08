namespace LSS {

  public class ConfigServos {

    public ConfigServosDetail [] Servos = new ConfigServosDetail[] { };

    public ConfigServosDetail GetPort(EZ_B.Servo.ServoPortEnum port) {

      foreach (var servo in Servos)
        if (servo.Port == port)
          return servo;

      return null;
    }
  }

  public class ConfigServosDetail {

    public EZ_B.Servo.ServoPortEnum Port;
  }
}
