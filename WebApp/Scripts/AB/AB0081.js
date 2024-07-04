
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var UrlReport = '/Report/A/AB0081.aspx';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //var st_getlogininfo = Ext.create('Ext.data.Store', {
    //    proxy: {
    //        type: 'ajax',
    //        actionMethods: {
    //            read: 'POST' // by default GET
    //        },
    //        url: '/api/AA0123/GetLoginInfo',
    //        reader: {
    //            type: 'json',
    //            rootProperty: 'etts'
    //        }
    //    },
    //    autoLoad: true
    //});
    var st_ym = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0093/GetYmCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    //var st_task = Ext.create('Ext.data.Store', {
    //    proxy: {
    //        type: 'ajax',
    //        actionMethods: {
    //            read: 'POST' // by default GET
    //        },
    //        url: '/api/AB0081/GetTaskCombo',
    //        reader: {
    //            type: 'json',
    //            rootProperty: 'etts'
    //        },
    //    },
    //    autoLoad: true
    //});

    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,

        items: [
        {
                xtype: 'combo',
                fieldLabel: '民國年月',
                name: 'P0',
                id: 'P0',
                store: st_ym,
                queryMode: 'local',
                displayField: 'VALUE',
                valueField: 'VALUE',
                allowBlank: false,
                fieldCls: 'required',
                width: 150,
                listeners: {
                    select: function (c, r, eo) {
                        Ext.getCmp('T1btn1').setDisabled(false);
                    }
                }
        //}, {
        //        xtype: 'combo',
        //        fieldLabel: '報表類型',
        //        name: 'P1',
        //        id: 'P1',
        //        store: st_task,
        //        queryMode: 'local',
        //        displayField: 'TEXT',
        //        valueField: 'VALUE',
        //        allowBlank: false,
        //        fieldCls: 'required',
        //        width: 150,
        //        listeners: {
        //            select: function (c, r, eo) {
        //                Ext.getCmp('T1btn1').setDisabled(false);
        //            }
        //        }
        },{
            xtype: 'button',
            text: '列印',
            id: 'T1btn1',
            disabled: true,
            handler: function () {
                msglabel('');
                var p0 = T1Query.getForm().findField('P0').getValue() == null ? '' : T1Query.getForm().findField('P0').getValue();
                //var p1 = T1Query.getForm().findField('P1').getValue() == null ? '' : T1Query.getForm().findField('P1').getValue();
                if (p0 == "") {
                    T1Query.getForm().findField('P0').focus();
                    Ext.Msg.alert('提醒', '民國年月及報表類型皆必選');
                }
                else {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var qstring = '?p0=' + p0; //  + '&p1=' + p1;
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
                msglabel('');
                Ext.getCmp('T1btn1').setDisabled(true);
                Ext.getDom('mainContent').src = '';
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '藥品 10908 上線',
            labelWidth: 150,
            labelSeparator: '',
            labelStyle: 'color: gray;',
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
        items: [
            {
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

    //T1Query.getForm().findField('P1').setValue('2');
    //T1Query.getForm().findField('P1').setValue(st_getlogininfo.getAt(0).get('TASK_ID'));
});