Ext.define('WEBAPP.model.AB0038VM', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'SEQ', type: 'string' }, //順序
        { name: 'CNAME', type: 'string' }, //中文名稱
        { name: 'ENAME', type: 'string' }, //英文名稱
        { name: 'CHK', type: 'date' }, //選擇FLAG
        { name: 'IP', type: 'date' }, //使用者IP
    ]
});