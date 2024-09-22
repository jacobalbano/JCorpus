SELECT  b.id					[BookId]
	   ,b.timestamp 			[Timestamp]
	   ,b.title 				[BookTitle]
	   ,b.path					[Path]
	   ,d.name					[Filename]
	   ,d.format				[Format]
	   ,r.rating				[Rating]
	   ,GROUP_CONCAT(a.name) 	[Authors]
	   ,stuffIds.ids			[Ids]
FROM books b
	INNER JOIN books_authors_link bal ON bal.book = b.id
	INNER JOIN authors a on bal.author = a.id
	INNER JOIN data d on d.book = b.id
	LEFT JOIN books_ratings_link brl ON brl.book = b.id
	LEFT JOIN ratings r on brl.rating = r.id
	INNER JOIN (
		SELECT ids.book 								[BookId], 
				GROUP_CONCAT(ids.type || ':' || ids.val) [Ids]
		FROM identifiers ids
		GROUP BY ids.book
	) stuffIds ON stuffIds.BookId = b.id
GROUP BY bal.book