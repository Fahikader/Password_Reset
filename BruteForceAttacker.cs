using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class BruteForceAttacker
{
    private readonly string _targetEncryptedPassword;
    private readonly int _maxThreads;
    private readonly string _salt = "static_salt_";
    private readonly char[] _characters = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("A1B2C3D4E5F6G7H8");
    private ConcurrentBag<string> _results = new ConcurrentBag<string>();
    private bool _found;

    public BruteForceAttacker(string targetEncryptedPassword, int maxThreads)
    {
        _targetEncryptedPassword = targetEncryptedPassword;
        _maxThreads = maxThreads;
    }

    public void StartAttack(Action<string, TimeSpan> onPasswordFound)
    {
        var startTime = DateTime.Now;
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;
        var tasks = new Task[_maxThreads];

        for (int i = 0; i < _maxThreads; i++)
        {
            tasks[i] = Task.Run(() => BruteForce(token), token);
        }

        Task.WaitAny(tasks);
        tokenSource.Cancel();

        var elapsedTime = DateTime.Now - startTime;
        if (_found)
        {
            onPasswordFound(_results.FirstOrDefault(), elapsedTime);
        }
        else
        {
            onPasswordFound(null, elapsedTime);
        }
    }

    private void BruteForce(CancellationToken token)
    {
        foreach (var guess in GenerateCombinations(_characters))
        {
            if (token.IsCancellationRequested) break;

            if (_found) break;

            if (CheckPassword(guess))
            {
                _results.Add(guess);
                _found = true;
                break;
            }
        }
    }

    private IEnumerable<string> GenerateCombinations(char[] characters)
    {
        var length = 1;
        while (!_found)
        {
            foreach (var combination in GenerateCombinations(characters, length))
            {
                yield return combination;
            }
            length++;
        }
    }

    private IEnumerable<string> GenerateCombinations(char[] characters, int length)
    {
        if (length == 1) return characters.Select(c => c.ToString());

        return GenerateCombinations(characters, length - 1)
            .SelectMany(t => characters, (t, c) => t + c);
    }

    private bool CheckPassword(string password)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = new byte[16]; // Initialization vector with zero padding

            var saltedPassword = password + _salt;
            byte[] bytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] encryptedBytes = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                var encryptedPassword = Convert.ToBase64String(encryptedBytes);
                return encryptedPassword == _targetEncryptedPassword;
            }
        }
    }
}

