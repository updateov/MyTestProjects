var CommentBox = React.createClass({displayName: 'CommentBox',
    render: function () {
        return (
            React.createElement('div', { className: "commentBox" },
                "Hello. world! I am a CommentBox.")
      //      <div className="commentBox">
      //          Hello, world! I am a CommentBox.
      //</div>
        );
    }
});
ReactDOM.render(
    //<CommentBox />,
    React.createElement(CommentBox, null),
    document.getElementById('content')
);