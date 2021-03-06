﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SparkyTestHelpers.Mapping
{
    /// <summary>
    /// Test helper for updating class instance properties with random values
    /// (usually for testing "mapping" from one type to another).
    /// </summary>
    public class RandomValuesHelper
    {
        /// <summary>
        /// Maximum number of items to generate for arrays / lists / IEnumerables.
        /// </summary>
        public int MaximumIEnumerableSize { get; set; } = 3;

        /// <summary>
        /// Maximum "depth" of "child" class instances to create.
        /// </summary>
        public int? MaximumDepth { get; set; }

        private static readonly Dictionary<Type, Func<Random, string, object>> _generatorDictionary
            = new Dictionary<Type, Func<Random, string, object>>
            {
                { typeof(DateTime), RandomDateTime },
                { typeof(DateTime?), RandomDateTime },
                { typeof(Guid), RandomGuid },
                { typeof(Guid?), RandomGuid },
                { typeof(bool), RandomBool },
                { typeof(bool?), RandomBool },
                { typeof(byte), RandomByte },
                { typeof(byte?), RandomByte },
                { typeof(byte[]), RandomByteArray },
                { typeof(sbyte), RandomSByte },
                { typeof(sbyte?), RandomSByte },
                { typeof(char), RandomChar },
                { typeof(char?), RandomChar },
                { typeof(decimal), RandomDecimal },
                { typeof(decimal?), RandomDecimal },
                { typeof(double), RandomDouble },
                { typeof(double?), RandomDouble },
                { typeof(float), RandomFloat },
                { typeof(float?), RandomFloat },
                { typeof(int), RandomInt },
                { typeof(int?), RandomInt },
                { typeof(uint), RandomUInt },
                { typeof(uint?), RandomUInt },
                { typeof(long), RandomLong },
                { typeof(long?), RandomLong },
                { typeof(ulong), RandomULong },
                { typeof(ulong?), RandomULong },
                { typeof(object), RandomString },
                { typeof(short), RandomShort },
                { typeof(short?), RandomShort },
                { typeof(ushort), RandomUShort },
                { typeof(ushort?), RandomUShort },
                { typeof(string), RandomString }
            };

        private static readonly TypeInfo _iEnumerableTypeInfo = typeof(IEnumerable).GetTypeInfo();
        private static readonly TypeInfo _iListTypeInfo = typeof(IList).GetTypeInfo();

        private Dictionary<Type, int> _createdTypes;

        /// <summary>
        /// Sets <see cref="MaximumIEnumerableSize"/> value.
        /// </summary>
        /// <param name="maximumIEnumerableSize">The maximum IENumerable size.</param>
        /// <returns>"This" <see cref="RandomValuesHelper"/> instance.</returns>
        // ReSharper disable once InconsistentNaming
        public RandomValuesHelper WithMaximumIENumerableSize(int maximumIEnumerableSize)
        {
            MaximumIEnumerableSize = maximumIEnumerableSize;
            return this;
        }

        /// <summary>
        /// Sets <see cref="MaximumDepth"/> value.
        /// </summary>
        /// <param name="maximumDepth">The maximum depth.</param>
        /// <returns>"This" <see cref="RandomValuesHelper"/> instance.</returns>
        public RandomValuesHelper WithMaximumDepth(int maximumDepth)
        {
            MaximumDepth = maximumDepth;
            return this;
        }

        /// <summary>
        /// Create an instance of the specified type and populate its properties with random values.
        /// </summary>
        /// <remarks>"Pseudonym" of <see cref="CreateRandom{T}"/></remarks>
        /// <typeparam name="T">The type of the instance for which properties are to be updated.</typeparam>
        /// <param name="callback">Optional "callback" function to perform additional property assignments.</param>
        /// <returns>New instance.</returns>
        public T CreateInstanceWithRandomValues<T>(Action<T> callback = null)
        {
            T instance = UpdatePropertiesWithRandomValues((T)Activator.CreateInstance(typeof(T)));

            callback?.Invoke(instance);

            return instance;
        }

        /// <summary>
        /// Create an instance of the specified type and populate its properties with random values.
        /// </summary>
        /// <remarks>"Pseudonym" of <see cref="CreateInstanceWithRandomValues{T}"/></remarks>
        /// <typeparam name="T">The type of the instance for which properties are to be updated.</typeparam>
        /// <param name="callback">Optional "callback" function to perform additional property assignments.</param>
        /// <returns>New instance.</returns>
        public T CreateRandom<T>(Action<T> callback = null)
        {
            return CreateInstanceWithRandomValues<T>(callback);
        }

        /// <summary>
        /// Update <typeparamref name="T"/> instance properties with random values.
        /// </summary>
        /// <typeparam name="T">The type of the instance for which properties are to be updated.</typeparam>
        /// <param name="instance">An instance of <typeparamref name="T"/>.</param>
        /// <returns>The <paramref name="instance"/> with updated property values.</returns>
        public T UpdatePropertiesWithRandomValues<T>(T instance)
        {
            var random = new Random();
            int depth = 1;
            _createdTypes = new Dictionary<Type, int>();

            UpdatePropertiesWithRandomValues(instance, random, typeof(T).GetTypeInfo(), depth);

            return instance;
        }

        private void UpdatePropertiesWithRandomValues(object instance, Random random, TypeInfo typeInfo, int depth)
        {
            foreach (PropertyInfo property in PropertyEnumerator.GetPublicInstanceReadWriteProperties(typeInfo))
            {
                var value = GetRandomValue(random, property.PropertyType, property.Name, depth);

                if (value != null)
                {
                    property.SetValue(instance, value, null);
                }
            }
        }

        private object GetRandomValue(Random random, Type typ, string prefix, int depth)
        {
            object value = null;
            TypeInfo typeInfo = typ.GetTypeInfo();

            if (_generatorDictionary.ContainsKey(typ))
            {
                value = _generatorDictionary[typ](random, prefix);
            }
            else if (typeInfo.IsArray)
            {
                value = RandomArray(random, typ.GetElementType(), prefix, depth + 1);
            }
            else if (_iListTypeInfo.IsAssignableFrom(typ))
            {
                value = RandomList(random, typeInfo, prefix, depth + 1);
            }
            else if (_iEnumerableTypeInfo.IsAssignableFrom(typ))
            {
                value = RandomIEnumerable(random, typeInfo, prefix, depth + 1);
            }
            else if (typeInfo.IsClass)
            {
                value = RandomClassInstance(random, typ, depth + 1);
            }
            else if (typeInfo.IsEnum)
            {
                value = RandomEnum(random, typ);
            }

            return value;
        }

        private Array RandomArray(Random random, Type typ, string prefix, int depth)
        {
            Array array = null;

            if (typ != null)
            {
                int itemCount = random.Next(1, MaximumIEnumerableSize);

                array = TryToCreateInstance(typ, () => Array.CreateInstance(typ, itemCount)) as Array;

                if (array != null)
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        string childPrefix = string.Format("{0}[{1}]", prefix, i);
                        var item = GetRandomValue(random, typ, childPrefix, depth);
                        array.SetValue(item, i);
                    }
                }
            }

            return array;
        }

        private object RandomClassInstance(Random random, Type typ, int depth)
        {
            if (MaximumDepth.HasValue && MaximumDepth.Value < depth)
            {
                return null;
            }

            object value = null;
            bool shouldCreateInstance;

            if (_createdTypes.ContainsKey(typ))
            {
                // If we've already created a type at a higher level, stop. Don't create an infinite loop.
                shouldCreateInstance = (_createdTypes[typ] == depth);
            }
            else
            {
                shouldCreateInstance = true;
                _createdTypes.Add(typ, depth);
            }

            if (shouldCreateInstance)
            {
                value = TryToCreateInstance(typ, () => Activator.CreateInstance(typ));

                if (value != null)
                {
                    UpdatePropertiesWithRandomValues(value, random, typ.GetTypeInfo(), depth);
                }
            }

            return value;
        }

        /// <summary>
        /// Get random Enum value.
        /// </summary>
        /// <typeparam name="TEnum">The Enum type.</typeparam>
        /// <returns>Random <typeparamref name="TEnum"/> value.</returns>
        public TEnum RandomEnumValue<TEnum>() where TEnum : struct, IConvertible
        {
            Type enumType = typeof(TEnum);

            if (!enumType.GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException($"{enumType.FullName} is not an Enum type.");
            }

            return (TEnum) RandomEnum(new Random(), enumType);
        }

        /// <summary>
        /// Get random bool value.
        /// </summary>
        /// <returns>Random <see cref="bool" /> value.</returns>
        public bool RandomBool()
        {
            return (bool)RandomBool(new Random(), null);
        }

        /// <summary>
        /// Get random byte value.
        /// </summary>
        /// <returns>Random <see cref="byte" /> value.</returns>
        public byte RandomByte()
        {
            return (byte)RandomByte(new Random(), null);
        }

        /// <summary>
        /// Get random char value.
        /// </summary>
        /// <returns>Random <see cref="char" /> value.</returns>
        public char RandomChar()
        {
            return (char)RandomChar(new Random(), null);
        }

        /// <summary>
        /// Get random DateTime value.
        /// </summary>
        /// <returns>Random <see cref="DateTime" /> value.</returns>
        public DateTime RandomDateTime()
        {
            return (DateTime)RandomDateTime(new Random(), null);
        }

        /// <summary>
        /// Get random decimal value.
        /// </summary>
        /// <returns>Random <see cref="decimal" /> value.</returns>
        public decimal RandomDecimal()
        {
            return (decimal)RandomDecimal(new Random(), null);
        }

        /// <summary>
        /// Get random double value.
        /// </summary>
        /// <returns>Random <see cref="double" /> value.</returns>
        public double RandomDouble()
        {
            return (double)RandomDouble(new Random(), null);
        }

        /// <summary>
        /// Get random Guid value.
        /// </summary>
        /// <returns>Random <see cref="Guid" /> value.</returns>
        public Guid RandomGuid()
        {
            return (Guid)RandomGuid(new Random(), null);
        }

        /// <summary>
        /// Get random int value.
        /// </summary>
        /// <returns>Random <see cref="int" /> value.</returns>
        public int RandomInt()
        {
            return (int)RandomInt(new Random(), null);
        }

        /// <summary>
        /// Get random uint value.
        /// </summary>
        /// <returns>Random <see cref="uint" /> value.</returns>
        public uint RandomUInt()
        {
            return (uint)RandomUInt(new Random(), null);
        }

        /// <summary>
        /// Get random short value.
        /// </summary>
        /// <returns>Random <see cref="short" /> value.</returns>
        public short RandomShort()
        {
            return (short)RandomShort(new Random(), null);
        }

        /// <summary>
        /// Get random ushort value.
        /// </summary>
        /// <returns>Random <see cref="ushort" /> value.</returns>
        public ushort RandomUShort()
        {
            return (ushort)RandomUShort(new Random(), null);
        }

        /// <summary>
        /// Get random long value.
        /// </summary>
        /// <returns>Random <see cref="long" /> value.</returns>
        public long RandomLong()
        {
            return (long)RandomLong(new Random(), null);
        }

        /// <summary>
        /// Get random ulong value.
        /// </summary>
        /// <returns>Random <see cref="ulong" /> value.</returns>
        public ulong RandomULong()
        {
            return (ulong)RandomULong(new Random(), null);
        }

        /// <summary>
        /// Get random float value.
        /// </summary>
        /// <returns>Random <see cref="float" /> value.</returns>
        public float RandomFloat()
        {
            return (float)RandomFloat(new Random(), null);
        }

        /// <summary>
        /// Get random sbyte value.
        /// </summary>
        /// <returns>Random <see cref="sbyte" /> value.</returns>
        public sbyte RandomSByte()
        {
            return (sbyte)RandomSByte(new Random(), null);
        }

        /// <summary>
        /// Get random string value.
        /// </summary>
        /// <param name="prefix">optional string prefix.</param>
        /// <returns>Random <see cref="string"/> value.</returns>
        public string RandomString(string prefix = null)
        {
            return $"{prefix}{RandomGuid()}";
        }

        private object RandomEnum(Random random, Type enumType)
        {
            Array values = Enum.GetValues(enumType);

            int index = random.Next(0, values.Length - 1);

            return values.GetValue(index);
        }

        private object RandomIEnumerable(Random random, TypeInfo typeInfo, string prefix, int depth)
        {
            Array array = null;
            Type[] types = typeInfo.GetGenericArguments();

            if (types.Length == 1)
            {
                array = RandomArray(random, types[0], prefix, depth);
            }

            return array;
        }

        private object RandomList(Random random, TypeInfo typeInfo, string prefix, int depth)
        {
            object list = null;

            Type[] genericArguments = typeInfo.GetGenericArguments();

            if (genericArguments.Length == 1)
            {
                Array array = RandomArray(random, genericArguments[0], prefix, depth);

                if (array != null)
                {
                    Type genericListType = typeof(List<>).MakeGenericType(genericArguments);

                    TryToCreateInstance(genericListType, () => list = Activator.CreateInstance(genericListType, array));
                }
            }

            return list;
        }

        private static object RandomBool(Random random, string prefix)
        {
            return (random.Next() % 2 == 0);
        }

        private static object RandomByteArray(Random random, string prefix)
        {
            var buffer = new byte[random.Next(10, 100)];
            random.NextBytes(buffer);
            return buffer;
        }

        private static object RandomByte(Random random, string prefix)
        {
            return ((byte[])RandomByteArray(random, prefix))[0];
        }

        private static object RandomChar(Random random, string prefix)
        {
            return random.Next().ToString()[0];
        }

        private static object RandomDateTime(Random random, string prefix)
        {
            return DateTime.Today.AddDays(random.Next(-1000, 1000));
        }

        private static object RandomDecimal(Random random, string prefix)
        {
            return Convert.ToDecimal(random.Next(1_000_000)) / 100;
        }

        private static object RandomDouble(Random random, string prefix)
        {
            return Convert.ToDouble(random.Next(1_000_000)) / 100;
        }

        private static object RandomGuid(Random random, string prefix)
        {
            return Guid.NewGuid();
        }

        private static object RandomInt(Random random, string prefix)
        {
            return random.Next();
        }

        private static object RandomUInt(Random random, string prefix)
        {
            return Convert.ToUInt32(random.Next(0, Int32.MaxValue));
        }

        private static object RandomShort(Random random, string prefix)
        {
            return Convert.ToInt16(random.Next(Int16.MinValue, Int16.MaxValue));
        }

        private static object RandomUShort(Random random, string prefix)
        {
            return Convert.ToUInt16(random.Next(UInt16.MinValue, UInt16.MaxValue));
        }

        private static object RandomLong(Random random, string prefix)
        {
            return (long)random.Next();
        }

        private static object RandomULong(Random random, string prefix)
        {
            return Convert.ToUInt64(random.Next(0, Int32.MaxValue));
        }

        private static object RandomFloat(Random random, string prefix)
        {
            return (float)Convert.ToDouble(random.Next(10_000_000)) / 1000; ;
        }

        private static object RandomSByte(Random random, string prefix)
        {
            return Convert.ToSByte(random.Next(Convert.ToInt32(sbyte.MinValue), Convert.ToInt32(sbyte.MaxValue)));
        }

        private static object RandomString(Random random, string prefix)
        {
            return string.Format("{0}{1}", prefix, random.Next());
        }

        private object TryToCreateInstance(Type type, Func<object> callback)
        {
            object value = null;

            try
            {
                value = callback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(RandomValuesHelper)} was unable to create a {type.FullName} instance: {ex.Message}.");
            }

            return value;
        }
    }
}
