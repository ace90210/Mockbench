using System;

namespace Mockbench.Shared
{
    public interface ICopyTo<T> where T : class
    {
        public int GetId();

        public T CopyTo(T target);
    }

    public class SnapshotEntity<T> : IDisposable where T : class, ICopyTo<T>, new()
    {
        private T _currentEntity;

        private SnapshotEntity<T> _previousEntity;

        public T Value 
        { 
            get => GetValue();
            set => SetNewValue(value);
        }

        public SnapshotEntity(T entity, bool initialisePrevious = true)
        {
            _currentEntity = entity;

            if (initialisePrevious && _currentEntity is not null)
            {
                _previousEntity = new SnapshotEntity<T>(_currentEntity.CopyTo(new()), false);
            }
        }

        private T GetValue() { return _currentEntity; }

        public T GetPreviousValue() { return _previousEntity.GetValue(); }

        private void SetNewValue(T value)
        {
            CommitChanges();
            _currentEntity = value;
        }

        public void CommitChanges()
        {
            if (_currentEntity is not null)
            {
                _currentEntity.CopyTo(_previousEntity.GetValue());
            }
        }

        public T RestoreValue()
        {
            DisposeCurrent();

            _previousEntity.GetValue().CopyTo(_currentEntity);

            return _currentEntity;
        }

        public void Dispose()
        {
            DisposeCurrent();
            DisposePrevious();
        }

        private void DisposeCurrent()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (_currentEntity is not null and IDisposable disposable)
            {
                disposable.Dispose();
                _currentEntity = default;
            }
        }

        private void DisposePrevious()
        {
            if (_previousEntity is not null)
            {
                ((IDisposable)_previousEntity).Dispose();
                _previousEntity = null;
            }
        }

        public override string ToString()
        {
            if (_previousEntity is not null)
                return $"\nCurrent: {_currentEntity?.ToString() ?? "[[Empty]]"}\nPrevious: {_previousEntity.GetValue()?.ToString() ?? "[[Empty]]"}\n";

            return $"\nCurrent: {_currentEntity?.ToString() ?? "[[Empty]]"}\nPrevious: [[Empty]]\n";
        }
    }
}