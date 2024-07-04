
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var UrlReport = '/Report/F/FA0072.aspx';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    
    var st_whmast = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0072/GetWhmastCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,

        items: [{
            xtype: 'monthfield',
            fieldLabel: '年月',
            name: 'P0',
            id: 'P0',
            width: 150,
            labelWidth: 70,
            padding: '0 4 0 4',
            fieldCls: 'required',
            value: new Date(),
            allowBlank: false
        }, {
            xtype: 'combo',
            fieldLabel: '庫房',
            name: 'P1',
            id: 'P1',
            width: 250,
            labelWidth: 70,
            store: st_whmast,
            queryMode: 'local',
            fieldCls: 'required',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            allowBlank: false,
            listeners: {
                change: function () {
                    Ext.getCmp('T1btn1').setDisabled(false);
                }
            }
        },
        {
            xtype: 'button',
            text: '列印',
            id: 'T1btn1',
            disabled: true,
            handler: function () {
                if (T1Query.getForm().findField("P0").getValue() == null || T1Query.getForm().findField("P0").getValue() == "" || T1Query.getForm().findField("P1").getValue() == null || T1Query.getForm().findField("P1").getValue() == "" ) {
                    Ext.Msg.alert('提醒', '年月及庫房不可空白');
                    msglabel("年月及庫房不可空白");
                }
                else {
                    var p0 = T1Query.getForm().findField('P0').rawValue == null ? '' : T1Query.getForm().findField('P0').rawValue;
                    var p1 = T1Query.getForm().findField('P1').getValue() == null ? '' : T1Query.getForm().findField('P1').getValue();
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var qstring = '?p0=' + p0 + '&p1=' + p1;

                    Ext.getDom('mainContent').src = UrlReport + qstring;

                    $('iframe#mainContent').load(function () {
                        myMask.hide();
                    });
                }
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                Ext.getCmp('T1btn1').setDisabled(true);
                Ext.getDom('mainContent').src = '';
            }
        }
        ]
    });

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        resizeTabs: true,
        layout: 'fit',
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
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
            itemId: 'tabReport',
            region: 'north',
            frame: false,
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [form1]
        }, {
            region: 'center',
            xtype: 'form',
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
        }
        ]
    });

});