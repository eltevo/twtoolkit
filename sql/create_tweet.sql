CREATE TABLE [$dbname].[dbo].[$tablename]
(
	[run_id] [smallint] NOT NULL,
	[tweet_id] [bigint] NOT NULL,
	[created_at] [datetime] NOT NULL,
	[utc_offset] [int] NULL,
	[user_id] [bigint] NOT NULL,
	[place_id] [char](16) NULL,
	[lon] [float] NULL,
	[lat] [float] NULL,
	[cx] [float] NOT NULL,
	[cy] [float] NOT NULL,
	[cz] [float] NOT NULL,
	[htm_id] [bigint] NOT NULL,
	[in_reply_to_tweet_id] [bigint] NULL,
	[in_reply_to_user_id] [bigint] NULL,
	[possibly_sensitive] [bit] NULL,
	[possibly_sensitive_editable] [bit] NULL,
	[retweet_count] [int] NOT NULL,
	[text] [nvarchar](150) NOT NULL,
	[truncated] [bit] NOT NULL,
	[lang] [char](5) NOT NULL,
	[lang_word_count] [tinyint] NOT NULL,
	[lang_guess1] [char](2) NOT NULL,
	[lang_guess2] [char](2) NOT NULL
) ON [PRIMARY];