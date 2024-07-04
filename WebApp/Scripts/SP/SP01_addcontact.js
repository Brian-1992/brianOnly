Ext.onReady(function () {
    var T1Form = Ext.widget({
        xtype: 'form',
        bodyStyle: 'margin:5px;border:none',
        layout: {
            type: 'vbox'
            //tdAttrs: { valign: 'top' }
        },
        width: 800,
        autoScroll: true,
        items: [
            {
                xtype: 'fieldset',
                //columnWidth: 0.5,
                title: '基本資料',
                autoHeight: true,
                width: 800,
                //cls: 'my-fieldset',
                style: "margin:5px;background-color: #ecf5ff;",
                cls: 'fieldset-title-bigsize',
                //collapsible: false,
                //defaultType: 'textfield',
                //defaults: { anchor: '70%' },
                layout: 'anchor',
                items: [
                    {
                        xtype: 'container',
                        //width: 800,
                        layout: {
                            type: 'table',
                            columns: 4
                        },
                        items: [
                            {
                                xtype: 'textfield',
                                //id: 'mlifnr',
                                fieldLabel: '聯絡人姓氏',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'mlifnr',
                                fieldLabel: '聯絡人名稱',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'combo',
                                //id: 'mlifnr',
                                colspan: 2,
                                store: ['先生', '小姐'],
                                fieldLabel: '稱謂',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                fieldLabel: '職稱',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'container',
                                colspan: 3,
                                layout: 'fit',
                                style: { marginBottom: '4px' },
                                items: [
                                    {
                                        xtype: 'textfield',
                                        //id: 'stceg',
                                        fieldLabel: '聯絡人備註',
                                        labelAlign: 'right'
                                    }
                                ]
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                fieldLabel: '電話1',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                fieldLabel: '分機1',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 2,
                                fieldLabel: '電話1備註',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                fieldLabel: '行動電話1',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 3,
                                fieldLabel: '行動電話1備註',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                style: 'margin-top:15px;',
                                fieldLabel: '電話2',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                style: 'margin-top:15px;',
                                fieldLabel: '分機2',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 2,
                                style: 'margin-top:15px;',
                                fieldLabel: '電話2備註',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                fieldLabel: '行動電話2',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 3,
                                fieldLabel: '行動電話2備註',
                                labelAlign: 'right'
                            },
                           {
                                xtype: 'textfield',
                                //id: 'stceg',
                                style: 'margin-top:15px;',
                                fieldLabel: '電話3',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                style: 'margin-top:15px;',
                                fieldLabel: '分機3',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 2,
                                style: 'margin-top:15px;',
                                fieldLabel: '電話3備註',
                                labelAlign: 'right'
                            }, 
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                style: 'margin-top:15px;',
                                fieldLabel: '電傳1',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                style: 'margin-top:15px;',
                                fieldLabel: '電傳1分機',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 2,
                                style: 'margin-top:15px;',
                                fieldLabel: '電傳1備註',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                fieldLabel: '電傳2',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                fieldLabel: '電傳2分機',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 2,
                                fieldLabel: '電傳2備註',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                style: 'margin-top:15px;',
                                fieldLabel: 'Email 1',
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                //id: 'stceg',
                                colspan: 2,
                                style: 'margin-top:15px;',
                                fieldLabel: 'Email 2',
                                labelAlign: 'right'
                            }
                        ]
                    }
                ]
            },
            {
                xtype: 'button',
                style: 'margin: 5px;',
                text: '新增',
                //iconCls: 'MGRPSearch',
                handler: function () {
                }
            }
        ]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 'form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Form]
        }]
    });
});