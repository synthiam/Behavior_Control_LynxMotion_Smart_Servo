using System.Text;

namespace LSS {

  public class ServoCommands {

    public enum BAUD_RATES {
      BAUD_500000 = 500000,
      BAUD_460800 = 460800,
      BAUD_250000 = 250000,
      BAUD_230400 = 230400,
      BAUD_115200 = 115200,
      BAUD_57600 = 57600,
      BAUD_38400 = 38400,
      BAUD_19200 = 19200,
      BAUD_9600 = 9600
    }

    public enum LED_COLORS {
      Off = 0,
      Red = 1,
      Green = 2,
      Blue = 3,
      Yellow = 4,
      Cyan = 5,
      Magenta = 6,
      White = 7
    }

    /// <summary>
    /// Change Baud Rate
    /// </summary>
    public byte[] ChangeBaudRate(byte id, BAUD_RATES baud) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}CB{1}\r", id, (int)baud));
    }

    /// <summary>
    /// Return the data packet that will release a servo with the specified ID
    /// </summary>
    public byte[] ReleaseServo(byte id) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}L\r", id));
    }

    /// <summary>
    /// Returns the data packet that will  move a servo with the specified id to the position
    /// </summary>
    public byte[] MoveServoCmd(byte id, int position) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}D{1}0\r", id, position));
    }

    /// <summary>
    /// Returns the data packet that contains the position of the servo
    /// </summary>
    public byte[] GetCurrentPositionCmd(byte id) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}QD\r", id));
    }

    public byte[] GetTemp(byte id) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}QT\r", id));
    }

    /// <summary>
    /// Return a packet that will set the speed of the servo with the id to the speed
    /// </summary>
    public byte[] ServoSpeed(byte id, int speed) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}SD{1}0\r", id, speed));
    }

    /// <summary>
    /// Change the LED status of the specified servo
    /// </summary>
    public byte[] LED(byte id, LED_COLORS color) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}LED{1}\r", id, (int)color));
    }

    public byte[] SendPing(byte id) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}QID\r", id));
    }

    public byte[] ChangeID(byte fromId, byte toId) {

      return Encoding.ASCII.GetBytes(string.Format("#{0}CID{1}\r", fromId, toId));
    }
  }
}
