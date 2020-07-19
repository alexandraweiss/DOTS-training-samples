﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;
using Random = Unity.Mathematics.Random;


namespace HighwayRacer
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(RoadSys))]
    [AlwaysUpdateSystem]
    public class CarBucketSpawnSys : SystemBase
    {
        public const float minSpeed = 7f;
        public const float maxSpeed = 20f; // max cruising speed

        public const float minBlockedDist = 9f;
        public const float maxBlockedDist = 15f;

        public const float minOvertakeModifier = 1.2f;
        public const float maxOvertakeModifier = 1.6f;

        public const float startSpeed = 6.0f;

        public static bool remakeBuckets = false;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        private void makeBuckets()
        {
            var seed = (uint) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var rand = new Random(seed);

            var carBuckets = RoadSys.CarBuckets;
            var roadSegments = RoadSys.roadSegments;

            var bucketIdx = 0;
            var bucket = carBuckets.GetCars(bucketIdx);
            var writer = carBuckets.GetWriter(bucketIdx);
            var segment = roadSegments[bucketIdx];

            Car car = new Car();
            car.LaneOffset = (half) 0.0f;
            car.OvertakeTimer = (half) 0.0f;
            car.Pos = 0.0f;
            car.RightMostLane();

            int nCars = RoadSys.numCars;
            // add cars to the buckets
            for (int i = 0; i < nCars; i++)
            {
                // while bucket is full
                while (bucket.Length >= bucket.Capacity ||
                       segment.IsCurved() ||
                       car.Pos >= (segment.Length - RoadSys.carSpawnDist - RoadSys.carSpawnDist))
                {
                    bucketIdx++;
                    car.Pos = 0.0f;
                    car.RightMostLane();
                    bucket = carBuckets.GetCars(bucketIdx);
                    writer = carBuckets.GetWriter(bucketIdx);
                    segment = roadSegments[bucketIdx];
                }

                // defensive check
                if (!carBuckets.IsBucketIdx(bucketIdx))
                {
                    Debug.LogError("ran out of buckets when adding cars. On car " + i); // shouldn't reach here
                    return;
                }

                car.DesiredSpeedUnblocked = (half) math.lerp(minSpeed, maxSpeed, rand.NextFloat());
                car.DesiredSpeedOvertake = new half(car.DesiredSpeedUnblocked * math.lerp(minOvertakeModifier, maxOvertakeModifier, rand.NextFloat()));

                car.Speed = startSpeed;
                car.BlockingDist = (half) math.lerp(minBlockedDist, maxBlockedDist, rand.NextFloat());
                car.CarState = CarState.Normal;

                writer.AddNoResize(car);

                car.SetNextPosAndLane();
            }

            carBuckets.Sort();
        }

        protected override void OnUpdate()
        {
            if (remakeBuckets)
            {
                makeBuckets();
                remakeBuckets = false;
            }
        }
    }
}