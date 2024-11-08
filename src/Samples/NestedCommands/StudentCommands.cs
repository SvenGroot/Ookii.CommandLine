using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace NestedCommands;

// This is the top-level "student" command. It has no functionality since everything is done in the
// ParentCommand class.
[Command("student")]
[Description("Add or remove a student.")]
internal class StudentCommand : ParentCommand
{
}

// Command to add students. Since it inherits from BaseCommand, it has a Path argument in addition
// to the arguments created here.
[GeneratedParser]
[Command("add")]
[ParentCommand(typeof(StudentCommand))]
[Description("Adds a student to the database.")]
internal partial class AddStudentCommand : BaseCommand
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The first name of the student.")]
    [ValidateNotWhiteSpace]
    public required string FirstName { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The last name of the student.")]
    [ValidateNotWhiteSpace]
    public required string LastName { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The student's major.")]
    public string? Major { get; set; }

    protected override async Task<int> RunAsync(Database db, CancellationToken cancellationToken)
    {
        int id = db.Students.Keys.Any() ? db.Students.Keys.Max() + 1 : 1;
        db.Students.Add(id, new Student(FirstName, LastName, Major, new()));
        await db.Save(Path, cancellationToken);
        Console.WriteLine($"Added a student with ID {id}.");
        return (int)ExitCode.Success;
    }
}

// Command to remove students. Since it inherits from BaseCommand, it has a Path argument in
// addition to the arguments created here.
[GeneratedParser]
[Command("remove")]
[ParentCommand(typeof(StudentCommand))]
[Description("Removes a student from the database.")]
internal partial class RemoveStudentCommand : BaseCommand
{

    [CommandLineArgument(IsPositional = true)]
    [Description("The ID of the student to remove.")]
    public required int Id { get; set; }

    protected override async Task<int> RunAsync(Database db, CancellationToken cancellationToken)
    {
        if (!db.Students.Remove(Id))
        {
            Console.WriteLine("No such student");
            return (int)ExitCode.IdError;
        }

        await db.Save(Path, cancellationToken);
        Console.WriteLine("Student removed.");
        return (int)ExitCode.Success;
    }
}

// Command to add a course to a student. Since it inherits from BaseCommand, it has a Path argument
// in addition to the arguments created here.
[GeneratedParser]
[Command("add-course")]
[ParentCommand(typeof(StudentCommand))]
[Description("Adds a course for a student.")]
internal partial class AddStudentCourseCommand : BaseCommand
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The ID of the student.")]
    public required int StudentId { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The ID of the course.")]
    public required int CourseId { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The grade achieved in the course.")]
    [ValidateRange(1.0f, 10.0f)]
    public required float Grade { get; set; }

    protected override async Task<int> RunAsync(Database db, CancellationToken cancellationToken)
    {
        if (!db.Students.TryGetValue(StudentId, out var student))
        {
            Console.WriteLine("No such student.");
            return (int)ExitCode.IdError;
        }

        if (!db.Courses.ContainsKey(CourseId))
        {
            Console.WriteLine("No such course.");
            return (int)ExitCode.IdError;
        }

        // You'd probably want to check for duplicates in a real application, but this is a sample,
        // not an actual database.
        student.Courses.Add(new(CourseId, Grade));
        await db.Save(Path, cancellationToken);
        return (int)ExitCode.Success;
    }
}
