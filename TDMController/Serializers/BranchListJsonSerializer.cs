using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using TDMController.Models;
using System.IO.Ports;
using TDMController.Models.TDMDevices;

namespace TDMController.Serializers
{
    internal class BranchListJsonConverter : JsonConverter<List<Branch>>
    {
        public override List<Branch> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonSerializer.Deserialize<JsonElement>(ref reader);
            var listOfBranches = new List<Branch>();

            if (jsonObject.TryGetProperty("Branches", out var branchesProperty))
            {
                foreach (var branch in branchesProperty.EnumerateArray())
                {
                    SerialPort? serialPort = null;
                    RotationDevice? rotationDevice = null;
                    IPositionDevice? positionDevice = null;
                    int? index = null;

                    if (branch.TryGetProperty("Com", out var comProperty))
                    {
                        serialPort = new SerialPort(comProperty.GetString(),9600);
                    }

                    if (branch.TryGetProperty("Index", out var indexProperty))
                    {
                        index = comProperty.GetInt32();
                    }

                    if (branch.TryGetProperty("MotorList", out var MotorsProperty))
                    {
                        foreach (var motor in MotorsProperty.EnumerateArray())
                        {
                            foreach (var motorProperty in motor.EnumerateObject())
                            {
                                var name = motorProperty.Name;
                                var props = motorProperty.Value;

                                switch (name)
                                {
                                    case "RotationMotor":
                                        rotationDevice = new RotationDevice(props.GetProperty("Direction").GetInt32(), serialPort!);
                                        break;

                                    case "PODLDevice":
                                        positionDevice = new PODLDevice(serialPort!);
                                        break;
                                }
                            }
                        }
                    }
                    var branchObject = new Branch(serialPort, index, rotationDevice, positionDevice);
                    listOfBranches.Add(branchObject);
                }
            }
            return listOfBranches;
        }

        public override void Write(Utf8JsonWriter writer, List<Branch> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteStartArray("Branches");
            foreach (var branch in value)
            {
                writer.WriteStartObject();
                writer.WriteString("Com", branch._serialPort.PortName);

                writer.WriteStartArray("MotorList");

                writer.WriteStartObject();
                writer.WritePropertyName(branch._rotationDevice.GetType().Name);
                var rotationDeviceJson = branch._rotationDevice.ToJson();
                writer.WriteRawValue(rotationDeviceJson);
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName(branch._positionDevice.GetType().Name);
                var positionDeviceJson = branch._positionDevice.ToJson();
                writer.WriteRawValue(positionDeviceJson);
                writer.WriteEndObject();

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
