insert into [User]
values('Karl', 'Nilsson')

insert into [User]
values('Oskar', 'Hansson')

insert into Post
values('Post nr1', GETDATE(), 1, 1)

insert into Post
values('Post nr2', GETDATE(), 1, 1)

insert into Post
values('Post nr3', GETDATE(), 0, 2)

insert into Post
values('Post nr4', GETDATE(), 1, 2)

insert into Comment
values('Comment nr1', GETDATE(), 1, 1, 2)

insert into Comment
values('Comment nr2', GETDATE(), 1, 1, 2)

insert into Comment
values('Comment nr1', GETDATE(), 1, 4, 1)

insert into Comment
values('Comment nr2', GETDATE(), 0, 4, 1)