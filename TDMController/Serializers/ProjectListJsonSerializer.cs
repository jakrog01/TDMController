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
    internal class ProjectJsonConverter : JsonConverter<Project>
    {
        public override Project Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonSerializer.Deserialize<JsonElement>(ref reader);
            var listOfBranches = new List<Branch>();

            int? photoBranchIndex = null;
            int? measureBranchIndex = null;

            Branch? measureBranch = null;
            Branch? photoBranch = null;

            if (jsonObject.TryGetProperty("MeasureBranch", out var measureIndex))
            {
                measureBranchIndex = measureIndex.GetInt32();
            }

            if (jsonObject.TryGetProperty("PhotoBranch", out var photoIndex))
            {
                photoBranchIndex = photoIndex.GetInt32();
            }

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

                    if (branch.TryGetProperty("BranchIndex", out var indexProperty))
                    {
                        index = indexProperty.GetInt32();
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

                    if (branchObject.BranchIndex == photoBranchIndex)
                    {
                        photoBranch = branchObject;
                    }
                    else if (branchObject.BranchIndex == measureBranchIndex)
                    {
                        measureBranch = branchObject;
                    }
                }
            }
            return new Project(listOfBranches, photoBranch, measureBranch);
        }

        public override void Write(Utf8JsonWriter writer, Project value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteStartArray("Branches");
            foreach (var branch in value.Branches)
            {
                writer.WriteStartObject();
                writer.WriteString("Com", branch.SerialPort.PortName);

                writer.WriteStartArray("MotorList");

                writer.WriteStartObject();
                writer.WritePropertyName(branch.RotationDevice.GetType().Name);
                var rotationDeviceJson = branch.RotationDevice.ToJson();
                writer.WriteRawValue(rotationDeviceJson);
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName(branch.PositionDevice.GetType().Name);
                var positionDeviceJson = branch.PositionDevice.ToJson();
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
