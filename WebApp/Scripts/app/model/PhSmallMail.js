Ext.define('WEBAPP.model.PhSmallMail', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'SEND_TO', type: 'string' },
        { name: 'MAIL_ADD', type: 'string' },
        { name: 'MEMO', type: 'string' }
    ]
});