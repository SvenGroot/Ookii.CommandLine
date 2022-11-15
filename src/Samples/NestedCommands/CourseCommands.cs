using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace NestedCommands;

// This is the top-level "course" command. It has no functionality since everything is done in the
// ParentCommand class.
[Command("course")]
[Description("Add or remove a course.")]
internal class CourseCommand : ParentCommand
{
}

// Command to add courses. Since it inherits from BaseCommand, it has a Path argument in addition
// to the arguments created here.
[Command("add")]
[ParentCommand(typeof(CourseCommand))]
[Description("Adds a course to the database.")]
internal class AddCourseCommand : BaseCommand
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("The first name of the course.")]
    [ValidateNotWhiteSpace]
    public string Name { get; set; } = string.Empty;

    [CommandLineArgument(Position = 1, IsRequired = true)]
    [Description("The name of the teacher of the course.")]
    [ValidateNotWhiteSpace]
    public string Teacher { get; set; } = string.Empty;

    protected override async Task<int> RunAsync(Database db)
    {
        int id = db.Courses.Any() ? db.Courses.Keys.Max() + 1 : 1;
        db.Courses.Add(id, new Course(Name, Teacher));
        await db.Save(Path);
        Console.WriteLine($"Added a course with ID {id}.");
        return (int)ExitCode.Success;
    }
}

// Command to remove courses. Since it inherits from BaseCommand, it has a Path argument in addition
// to the arguments created here.
[Command("remove")]
[ParentCommand(typeof(CourseCommand))]
[Description("Removes a course from the database.")]
internal class RemoveCourseCommand : BaseCommand
{

    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("The ID of the course to remove.")]
    public int Id { get; set; }

    protected override async Task<int> RunAsync(Database db)
    {
        if (db.Students.Any(s => s.Value.Courses.Any(c => c.CourseId == Id)))
        {
            Console.WriteLine("Can't remove a course referenced by a student.");
            return (int)ExitCode.IdError;
        }

        if (!db.Courses.Remove(Id))
        {
            Console.WriteLine("No such course");
            return (int)ExitCode.IdError;
        }

        await db.Save(Path);
        Console.WriteLine("Course removed.");
        return (int)ExitCode.Success;
    }
}
