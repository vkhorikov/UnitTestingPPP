using System;

namespace Book.Chapter10.Migrations
{
    [Migration(1)]
    public class CreateUserTable : Migration
    {
        public override void Up()
        {
            Create.Table("Users");
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }

    public class Delete
    {
        public static void Table(string users)
        {
            throw new NotImplementedException();
        }
    }

    public class Create
    {
        public static void Table(string users)
        {
            throw new NotImplementedException();
        }
    }

    public class Migration
    {
        public virtual void Up()
        {
            throw new NotImplementedException();
        }

        public virtual void Down()
        {
            throw new NotImplementedException();
        }
    }

    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(int i)
        {
            throw new NotImplementedException();
        }
    }
}
