/*
use master
drop DATABASE GroupManagementDB
*/


create DATABASE GroupManagementDB
go

use GroupManagementDB
go

create table groups(
id_group INT NOT NULL PRIMARY KEY IDENTITY,--עולה אוטומטי
name VARCHAR (100) NOT NULL,
created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
)
go


insert into groups  (name) values ('Netanya'),('Paradise')
select * from groups
go


--drop TABLE user_permissions
create TABLE user_Permissions(
	permission_code int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	permission_desc varchar (15),
)
GO



--drop TABLE users
create TABLE users(
	email varchar (40) not NULL,
	id int IDENTITY(1,1) NOT NULL,
	first_name varchar (15)not NULL,
	last_name varchar (15)not NULL,
	password VARCHAR(150) NOT NULL CHECK (LEN(Password) BETWEEN 3 AND 150),
	permission_code int DEFAULT 1,

	CONSTRAINT pk_Users PRIMARY KEY (email),
	CONSTRAINT FK_permission FOREIGN KEY (permission_code) REFERENCES user_Permissions (permission_code ),
)
GO




insert into user_Permissions values ('regular')
insert into user_Permissions values ('admin')
go

insert into users  (email,first_name,last_name,password,permission_code) values ('theAdmin@gmail.com','super','admin','MzAwMzE4MzIyMjgxMzExMzEzMzAyMzA5MzIzMzIxMzE2MjA1MjU4MjQyMjU5MjY0Mjcw',2)
go



select * from users
go






create TABLE user_loggedin(
	email varchar (40) not NULL,
	id int  NOT NULL,
	first_name varchar (15)not NULL,
	last_name varchar (15)not NULL,
	permission_code int ,
)
go


CREATE PROC   Proc_Del_Tbl_userloggedin AS   Delete From  user_loggedin
go



create PROC   Proc_User_loggedin
@email varchar (40),
@id int ,
@first_name varchar (15),
@last_name varchar (15),
@permission_code int
AS
exec Proc_Del_Tbl_userloggedin
INSERT user_loggedin values (@email, @id, @first_name,@last_name,@permission_code)
go






--drop TABLE users_groups
create TABLE users_groups(
	email varchar (40)  NOT NULL,
	id_group INT NOT NULL,
	CONSTRAINT pk_users_groups PRIMARY KEY (email,id_group),
    CONSTRAINT FK_users_groups FOREIGN KEY (email) REFERENCES users (email ),
	CONSTRAINT FK_groupsid_groups FOREIGN KEY (id_group) REFERENCES groups (id_group ),
)
GO


create view Show_My_Groups
as
SELECT groups.id_group, groups.name, groups.created_at
FROM     groups  right OUTER JOIN
                  users_groups ON groups.id_group = users_groups.id_group right OUTER JOIN
                  user_loggedin ON users_groups.email = user_loggedin.email
go



create PROC   Proc_User_Groups_List
as
SELECT * from Show_My_Groups
go

exec Proc_User_Groups_List
go

create PROC Proc_Delete_Group
@id_group int
as
delete from users_groups where @id_group = id_group
DELETE FROM groups Where id_group=@id_group
go

exec Proc_Delete_Group 2
select * from user_loggedin
go




create PROC Proc_Leave_Group
@id_group int
as
DELETE from dbo.users_groups 
FROM            dbo.user_loggedin INNER JOIN
                         dbo.users_groups ON dbo.user_loggedin.email = dbo.users_groups.email
						 where id_group = @id_group
go


create PROC Proc_Groups_To_Join
as
SELECT        dbo.groups.id_group, dbo.groups.name, dbo.groups.created_at
FROM            dbo.groups LEFT OUTER JOIN
                         dbo.users_groups ON dbo.groups.id_group = dbo.users_groups.id_group LEFT OUTER JOIN
                         dbo.Show_My_Groups ON dbo.groups.id_group = dbo.Show_My_Groups.id_group LEFT OUTER JOIN
                         dbo.user_loggedin ON dbo.users_groups.email = dbo.user_loggedin.email
WHERE        (dbo.Show_My_Groups.id_group IS NULL)
go








 
