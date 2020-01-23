using NUnit.Framework;
using UnityEngine;
using MLAgents.Sensor;

namespace MLAgents.Tests
{
    public class SensorTestHelper
    {
        public static void CompareObservation(ISensor sensor, float[] expected)
        {
            var numExpected = expected.Length;
            const float fill = -1337f;
            var output = new float[numExpected];
            for (var i = 0; i < numExpected; i++)
            {
                output[i] = fill;
            }
            Assert.AreEqual(fill, output[0]);

            WriteAdapter writer = new WriteAdapter();
            writer.SetTarget(output, 0);

            // Make sure WriteAdapter didn't touch anything
            Assert.AreEqual(fill, output[0]);

            sensor.Write(writer);
            for (var i = 0; i < numExpected; i++)
            {
                Assert.AreEqual(expected[i], output[i]);
            }
        }
    }

    public class VectorSensorTests
    {
        [Test]
        public void TestCtor()
        {
            ISensor sensor = new VectorSensor(4);
            Assert.AreEqual("VectorSensor_size4", sensor.GetName());

            sensor = new VectorSensor(3, "test_sensor");
            Assert.AreEqual("test_sensor", sensor.GetName());
        }

        [Test]
        public void TestWrite()
        {
            var sensor = new VectorSensor(4);
            sensor.AddObservation(1f);
            sensor.AddObservation(2f);
            sensor.AddObservation(3f);
            sensor.AddObservation(4f);

            SensorTestHelper.CompareObservation(sensor, new[] { 1f, 2f, 3f, 4f });
            // Check that if we don't call Update(), the same observations are produced
            SensorTestHelper.CompareObservation(sensor, new[] { 1f, 2f, 3f, 4f });

            // Check that Update() clears the data
            sensor.Update();
            SensorTestHelper.CompareObservation(sensor, new[] { 0f, 0f, 0f, 0f });

        }

        [Test]
        public void TestAddObservationFloat()
        {
            var sensor = new VectorSensor(1);
            sensor.AddObservation(1.2f);
            SensorTestHelper.CompareObservation(sensor, new []{1.2f});
        }

        [Test]
        public void TestAddObservationInt()
        {
            var sensor = new VectorSensor(1);
            sensor.AddObservation(42);
            SensorTestHelper.CompareObservation(sensor, new []{42f});
        }

        [Test]
        public void TestAddObservationVec()
        {
            var sensor = new VectorSensor(3);
            sensor.AddObservation(new Vector3(1,2,3));
            SensorTestHelper.CompareObservation(sensor, new []{1f, 2f, 3f});

            sensor = new VectorSensor(2);
            sensor.AddObservation(new Vector2(4,5));
            SensorTestHelper.CompareObservation(sensor, new[] { 4f, 5f });
        }

        [Test]
        public void TestAddObservationQuaternion()
        {
            var sensor = new VectorSensor(4);
            sensor.AddObservation(Quaternion.identity);
            SensorTestHelper.CompareObservation(sensor, new []{0f, 0f, 0f, 1f});
        }

        [Test]
        public void TestWriteEnumerable()
        {
            var sensor = new VectorSensor(4);
            sensor.AddObservation(new [] {1f, 2f, 3f, 4f});

            SensorTestHelper.CompareObservation(sensor, new[] { 1f, 2f, 3f, 4f });
        }

        [Test]
        public void TestAddObservationBool()
        {
            var sensor = new VectorSensor(1);
            sensor.AddObservation(true);
            SensorTestHelper.CompareObservation(sensor, new []{1f});
        }

        [Test]
        public void TestAddObservationOneHot()
        {
            var sensor = new VectorSensor(4);
            sensor.AddOneHotObservation(2, 4);
            SensorTestHelper.CompareObservation(sensor, new []{0f, 0f, 1f, 0f});
        }

        [Test]
        public void TestWriteTooMany()
        {
            var sensor = new VectorSensor(2);
            sensor.AddObservation(new [] {1f, 2f, 3f, 4f});

            SensorTestHelper.CompareObservation(sensor, new[] { 1f, 2f});
        }

        [Test]
        public void TestWriteNotEnough()
        {
            var sensor = new VectorSensor(4);
            sensor.AddObservation(new [] {1f, 2f});

            // Make sure extra zeros are added
            SensorTestHelper.CompareObservation(sensor, new[] { 1f, 2f, 0f, 0f});
        }
    }
}
