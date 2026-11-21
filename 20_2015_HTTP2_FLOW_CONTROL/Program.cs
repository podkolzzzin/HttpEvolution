// 20_2015_HTTP2_FLOW_CONTROL: HTTP/2: вікна flow control.
Http2Server.Run(stream => new Http2FlowControlConnection(stream));
