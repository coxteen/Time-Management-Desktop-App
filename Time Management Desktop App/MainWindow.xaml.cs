using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Time_Management_Desktop_App
{
    public partial class MainWindow : Window
    {
        private TaskManager _taskManager;

        public MainWindow()
        {
            InitializeComponent();
            _taskManager = new TaskManager();
            DataContext = _taskManager;
            LoadTasks();
            ClearInputs();
        }

        private async void AddTask_Click(object sender, RoutedEventArgs e)
        {
            int selectedHour = Convert.ToInt32((HoursComboBox.SelectedItem as ComboBoxItem)?.Content);
            int selectedMinute = Convert.ToInt32((MinutesComboBox.SelectedItem as ComboBoxItem)?.Content);

            DateTime selectedDate = DeadlinePicker.SelectedDate ?? DateTime.Now;

            DateTime deadlineWithTime = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                selectedHour,
                selectedMinute,
                0
            );

            TaskItem newTask = new TaskItem
            {
                Title = TaskInput.Text,
                Description = DescriptionInput.Text,
                Deadline = deadlineWithTime,
                Priority = (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Low",
                IsCompleted = false
            };

            await FirebaseHelper.AddTaskAsync(newTask);
            LoadTasks();
            ClearInputs();
        }

        private async void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is TaskItem selectedTask)
            {
                TaskItem editedTask = new TaskItem
                {
                    Title = TaskInput.Text,
                    Description = DescriptionInput.Text,
                    Deadline = DeadlinePicker.SelectedDate ?? DateTime.Now,
                    Priority = (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Low",
                    IsCompleted = selectedTask.IsCompleted
                };

                var tasks = await FirebaseHelper.GetTasksAsync();
                var taskToEdit = tasks.FirstOrDefault(t => t.Title == selectedTask.Title);
                if (taskToEdit != null)
                {
                    var key = taskToEdit.Key; // Adjust based on your implementation
                    await FirebaseHelper.UpdateTaskAsync(key, editedTask);
                }
                LoadTasks();
                ClearInputs();
            }
            else
            {
                MessageBox.Show("Please select a task to edit.");
            }
        }

        private async void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is TaskItem selectedTask)
            {
                var tasks = await FirebaseHelper.GetTasksAsync();
                var taskToDelete = tasks.FirstOrDefault(t => t.Title == selectedTask.Title);
                if (taskToDelete != null)
                {
                    var key = taskToDelete.Key;
                    await FirebaseHelper.DeleteTaskAsync(key);
                }
                LoadTasks();
            }
            else
            {
                MessageBox.Show("Please select a task to delete.");
            }
        }

        public async void LoadTasks()
        {
            var tasks = await FirebaseHelper.GetTasksAsync();

            List<TaskItem> sortedTasks = new List<TaskItem>();
            foreach (var task in tasks)
            {
                sortedTasks.Add(task);
            }

            sortedTasks.Sort(new CustomComparator.PriorityAndDeadlineComparator());
            _taskManager.Tasks.Clear();

            foreach (var task in sortedTasks)
            {
                _taskManager.Tasks.Add(task);
            }
        }

        private void ClearInputs()
        {
            TaskInput.Clear();
            DescriptionInput.Clear();
            DeadlinePicker.SelectedDate = null;
            PriorityComboBox.SelectedIndex = 0;
            HoursComboBox.SelectedIndex = 0;
            MinutesComboBox.SelectedIndex = 0;
        }
    }
}
