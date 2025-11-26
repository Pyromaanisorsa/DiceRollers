import json
import boto3 #Python API for AWS infrastructure services

dynamodb = boto3.resource('dynamodb') #connect to dynamodb
table = dynamodb.Table('RollRequests')

def lambda_handler(event, context):
    if 'body' not in event:
        return {
            'statusCode': 400,
            'body': json.dumps({'message': 'Missing request body.'})
        }

    body = json.loads(event['body'])
    player_id = body['playerID']

    response = table.get_item(
        Key={'playerID': player_id}
    )

    if 'Item' in response:
        status = response['Item'].get('status')
        if status == 'Completed':
            value = int(response['Item'].get('value'))
            return {
                'statusCode': 200,
                'body': json.dumps({'value': value})
            }
        else:
            return {
                'statusCode': 202,
                'body': json.dumps({'message': 'Roll still pending.'})
            }
    else:
        return {
            'statusCode': 404,
            'body': json.dumps('No roll found for playerID.')
        }