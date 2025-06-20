import { Card, CardContent } from '@/components/ui/card';
import { getConfig } from '@/lib/config';
import React, { useState, useEffect, useRef } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

interface DataPoint {
    timestamp: number;
    value: number;
}

interface SubscriptionData {
    data: {
        userCreated: {
            values: DataPoint[];
        };
    };
}

interface WebSocketMessage {
    type: string;
    id?: string;
    payload?: any;
}

interface GraphQLSubscription {
    query: string;
    variables?: Record<string, AggregationType>;
}

type ConnectionStatus = 'Disconnected' | 'Connecting' | 'Connected' | 'Error' | 'Completed';
type AggregationType = 'RAW' | 'ONE_MINUTE' | 'FIVE_MINUTES' | 'FIFTEEN_MINUTES' | 'ONE_HOUR';

const RealtimeChart: React.FC = () => {
    const [data, setData] = useState<DataPoint[]>([]);
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('Disconnected');
    const wsRef = useRef<WebSocket | null>(null);

    // GraphQL subscription query
    const subscriptionQuery: GraphQLSubscription = {
        query: `
          subscription($aggregation: AggregationType!) {
            userCreated(aggregation: $aggregation) {
                values {
                    timestamp
                    value
                }
            }
        }`,
        variables: {
            aggregation: 'RAW'
        }
    };

    const connectWebSocket = (url: string): void => {
        try {
            // Close existing connection if any
            if (wsRef.current) {
                wsRef.current.close();
            }

            setConnectionStatus('Connecting');
            const ws = new WebSocket(url, 'graphql-ws');
            wsRef.current = ws;

            ws.onopen = () => {
                setConnectionStatus('Connected');

                // Send connection init message (GraphQL-WS protocol)
                ws.send(JSON.stringify({
                    type: 'connection_init'
                }));
            };

            ws.onmessage = (event: MessageEvent) => {
                try {
                    const message: WebSocketMessage = JSON.parse(event.data);

                    switch (message.type) {
                        case 'connection_ack':
                            // Connection acknowledged, start subscription
                            ws.send(JSON.stringify({
                                id: 'subscription-1',
                                type: 'start',
                                payload: subscriptionQuery
                            }));
                            break;

                        case 'data':
                            // Handle subscription data - replace entire dataset
                            if (message.payload?.data?.userCreated?.values) {
                                const subscriptionData: SubscriptionData = message.payload;
                                const newValues = subscriptionData.data.userCreated.values;

                                // Sort the full dataset by timestamp
                                const sortedData = [...newValues].sort((a, b) => a.timestamp - b.timestamp);

                                // Replace the entire dataset (since subscription returns full data)
                                setData(sortedData);
                            }
                            break;

                        case 'error':
                            console.error('GraphQL subscription error:', message.payload);
                            setConnectionStatus('Error');
                            break;

                        case 'complete':
                            setConnectionStatus('Completed');
                            break;
                    }
                } catch (parseError) {
                    console.error('Failed to parse WebSocket message:', parseError);
                }
            };

            ws.onclose = (event: CloseEvent) => {
                setConnectionStatus('Disconnected');
                console.log('WebSocket closed:', event.reason);
            };

            ws.onerror = (error: Event) => {
                console.error('WebSocket error:', error);
                setConnectionStatus('Error');
            };

        } catch (error) {
            console.error('Failed to connect:', error);
            setConnectionStatus('Error');
        }
    };

    const disconnect = (): void => {
        if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
            wsRef.current.send(JSON.stringify({
                id: 'subscription-1',
                type: 'stop'
            }));

            wsRef.current.close();
            wsRef.current = null;
        }
    };

    useEffect(() => {
        const timer = setTimeout(async () => {
            const config = await getConfig();
            connectWebSocket(config.apiSocketUrl + config.graphQlPath);
        }, 100); // Small delay to avoid React strict mode issues

        return () => {
            clearTimeout(timer);
            disconnect();
        };
    }, []);

    return (

        <Card>
            <CardContent className="pt-6">
                <div className="text-center">
                    <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                        Analytics Dashboard
                    </h3>
                </div>
                <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={data.slice(0, 100)}>
                        {/* <CartesianGrid stroke="#eee" strokeDasharray="5 5" /> */}
                        <XAxis dataKey="time" />
                        <YAxis />
                        <Tooltip />
                        <Line type="monotone" dataKey="value" stroke="#8884d8" />
                    </LineChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>


    );
};

export default RealtimeChart;
