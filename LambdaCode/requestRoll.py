import json
import  uuid #for generating rollID 
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

    table.put_item(
        Item={
            'playerID': player_id,
            'status': 'Pending',
            'value': None
        }
    )

    return {
        'statusCode': 200,
        'body': json.dumps({'message': 'Roll request submitted successfully'})
    }