using CustomComparator;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Time_Management_Desktop_App
{
    public class TaskItem
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime Deadline { get; set; }
        public required string Priority { get; set; }
        public bool IsCompleted { get; set; }
        public string Key { get; set; } // Add this property to handle Firebase key
    }

    public class FirebaseHelper
    {
        private static FirebaseClient firebase = new("https://timemanagementappdatabase-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task AddTaskAsync(TaskItem task)
        {
            await firebase
                .Child("tasks")
                .PostAsync(task);
        }

        public static async Task<List<TaskItem>> GetTasksAsync()
        {
            var items = await firebase
                .Child("tasks")
                .OnceAsync<TaskItem>();

            return items.Select(item => new TaskItem
            {
                Title = item.Object.Title,
                Description = item.Object.Description,
                Deadline = item.Object.Deadline,
                Priority = item.Object.Priority,
                IsCompleted = item.Object.IsCompleted,
                Key = item.Key // Capture the key from Firebase
            }).ToList();
        }

        public static async Task UpdateTaskAsync(string key, TaskItem task)
        {
            await firebase
                .Child("tasks")
                .Child(key)
                .PutAsync(task);
        }

        public static async Task DeleteTaskAsync(string key)
        {
            await firebase
                .Child("tasks")
                .Child(key)
                .DeleteAsync();
        }
    }

    public class TaskManager : INotifyPropertyChanged
    {
        private ObservableCollection<TaskItem> _tasks = new ObservableCollection<TaskItem>();

        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static bool IsEmpty(string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        public void AddTask(TaskItem task)
        {
            if (!IsEmpty(task.Title) || !IsEmpty(task.Description))
            {
                if (IsEmpty(task.Title))
                {
                    task.Title = "No title";
                }
                if (IsEmpty(task.Description))
                {
                    task.Description = "No description";
                }

                Tasks.Add(task);
                SortTasks();
            }
        }

        public void RemoveTask(TaskItem task)
        {
            Tasks.Remove(task);
            SortTasks();
        }

        public void EditTask(TaskItem oldTask, TaskItem newTask)
        {
            var index = Tasks.IndexOf(oldTask);
            if (index >= 0)
            {
                Tasks[index] = newTask;
                SortTasks();
            }
        }

        private void SortTasks()
        {
            var sortedTasks = _tasks.OrderBy(t => t, new PriorityAndDeadlineComparator()).ToList();
            _tasks.Clear();
            foreach (var task in sortedTasks)
            {
                _tasks.Add(task);
            }
        }
    }
}
