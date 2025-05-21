using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
public class ThreadHandle
{
    public delegate void ThreadFunc(uint idx_start, uint idx_end, ref uint totalProgress, ref bool isDone);
    private ThreadFunc thfunc;
    private uint _idx_start;
    private uint _idx_end;

    private bool _isDone;

    public ThreadHandle(uint idx_start, uint idx_end, ThreadFunc _thfunc, ref bool isDone)
    {
        _idx_start = idx_start;
        _idx_end = idx_end;
        thfunc = _thfunc;

        _isDone = isDone;
    }

    public void func(ref uint totalProgress)
    {
        thfunc(_idx_start, _idx_end, ref totalProgress, ref _isDone);
    }
}

class ThreadSegment 
{
    public static uint limiteThreads = 7; // nombre de threads maximum
    private uint _length;
    private uint _threadCount;
    private uint _segmentLength;
    private uint _segmentRemainder;

    public uint totalProgress = 0;


    private Thread[] _threads;
    private bool[] _isThreadDone;

    public ThreadSegment(uint Length )
    {
        _length = Length;
        _threadCount =  (uint)Environment.ProcessorCount - 1; // -1 pour ne pas saturer le CPU
        if (_threadCount == 0)
        {
            _threadCount = 1; // au moins un thread
        }
        if (_threadCount > Length)
        {
            _threadCount = Length; // on ne peut pas avoir plus de threads que d'éléments
        }
        if (_threadCount > ThreadSegment.limiteThreads)
        {
            _threadCount = ThreadSegment.limiteThreads; // on ne peut pas avoir plus de threads que la limite
        }
        if (_threadCount < 1)
        {
            _threadCount = 1; // au moins un thread
        }
        if (_threadCount == 1)
        {
            _segmentLength = Length; // si un seul thread, on prend tout
            _segmentRemainder = 0;
        }
        else
        {
            _segmentLength = (uint)Mathf.Floor(Length / _threadCount); //arrondire a l'entier inferieur
            _segmentRemainder = Length % _threadCount;
        }

        _threads = new Thread[_threadCount];
        _isThreadDone = new bool[_threadCount];

        for (int i = 0; i < _threadCount; i++)
        {
            _isThreadDone[i] = false;
        }
    }

    public uint get_nThreads()
    {
        return _threadCount;
    }

    public void Execute(ThreadHandle.ThreadFunc _thfunc)
    {
        uint idx_start = 0;
        uint idx_end = 0;

        totalProgress = 0;
        
        for (uint i = 0; i < _threadCount; i++)
        {
            
            idx_start = i * _segmentLength   ;

            idx_end = (i + 1) * _segmentLength ;

            if (i == _threadCount - 1)
            {
                idx_end += _segmentRemainder;
            }

    
            try
            {
                ThreadHandle threadHandle = new ThreadHandle(idx_start, idx_end, _thfunc , ref _isThreadDone[i]);
                _threads[i] = new Thread(() => threadHandle.func(ref totalProgress));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Thread creation failed: " + e.Message);
                return;
            }
            

            
        }

        for (int i = 0; i < _threadCount; i++)
        {
            try
            {
                _threads[i].Start();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Thread start failed: " + e.Message);

                return;
            }
        }
    }

    public bool inProcess()
    {
        for (int i = 0; i < _threadCount; i++)
        {
            if (_threads[i].IsAlive)
            {
                return true;
            }
        }
        return false;
    }

    public void WaitForAllThreads()
    {
        for (int i = 0; i < _threadCount; i++)
        {
            _threads[i].Join();
        }
    }


}
