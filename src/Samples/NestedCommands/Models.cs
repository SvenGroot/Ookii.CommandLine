// Models used by the students "database".
using System.Text.Json;

namespace NestedCommands;

internal record class Student(string FirstName, string LastName, string? Major, List<StudentCourse> Courses);

internal record class Course(string Name, string Teacher);

internal record class StudentCourse(int CourseId, float Grade);

internal record class Database(Dictionary<int, Student> Students, Dictionary<int, Course> Courses)
{
    public static async Task<Database> Load(string path)
    {
        Database? result = null;
        try
        {
            using var stream = File.OpenRead(path);
            result = await JsonSerializer.DeserializeAsync<Database>(stream);
        }
        catch (FileNotFoundException)
        {
        }

        return result ?? new Database(new(), new());
    }

    public async Task Save(string path)
    {
        using var stream = File.Create(path);
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        await JsonSerializer.SerializeAsync(stream, this, options);
    }
}