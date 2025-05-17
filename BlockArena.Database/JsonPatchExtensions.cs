using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using MongoDB.Driver;
using System;
using System.Linq;

namespace BlockArena.Database
{
    public static class JsonPatchExtensions
    {
        public static UpdateDefinition<T> ToMongoUpdate<T>(this JsonPatchDocument<T> jsonPatch) where T : class
        {
            ArgumentNullException.ThrowIfNull(jsonPatch);

            var updates = jsonPatch.Operations
                .Select(ConvertOperationToUpdate)
                .ToList();

            return Builders<T>.Update.Combine(updates);
        }

        private static UpdateDefinition<T> ConvertOperationToUpdate<T>(Operation<T> operation) where T : class
        {
            var field = GetField(operation.path.Trim('/').Replace("/", "."));
            var value = operation.value;

            return operation.op switch
            {
                "add" => Builders<T>.Update.Set(field, value),
                "replace" => Builders<T>.Update.Set(field, value),
                "remove" => Builders<T>.Update.Unset(field),
                "copy" => throw new NotImplementedException("Копирование не поддерживается"),
                "move" => throw new NotImplementedException("Перемещение не поддерживается"),
                "test" => throw new NotImplementedException("Тестирование не поддерживается"),
                _ => throw new NotSupportedException($"Операция {operation.op} не поддерживается"),
            };
        }

        private static string GetField(string path)
        {
            return path.TrimStart('/');
        }
    }
}