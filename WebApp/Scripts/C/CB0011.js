Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['BOXNO', 'BARCODE', 'DESCRIPT', 'XCATEGORY', 'STATUS', 'CREATE_TIME', 'CREATE_USER', 'UPDATE_TIME', 'UPDATE_IP', 'UPDATE_USER', {
            name: 'BARCODE_ICON',
            defaultValue: '123'
        }]
    });
    var BoxStore = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        autoLoad: false,
        remoteSort: true,
        sorters: [{ property: 'BOXNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                //read: 'POST' // by default GET
            },
            url: '/api/CB0011/GetBox',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, operation, eOpts) {
                store.getProxy().extraParams = {
                    BOXNO: Ext.getCmp('BOXNO').getValue(),
                    BARCODE: Ext.getCmp('BARCODE').getValue(),
                    STATUS: Ext.getCmp('STATUS').getValue()
                };
            },
            load: function (store, operation, eOpts) {
                if (store.getCount() > 0) {
                    Ext.getCmp('printbtn').setDisabled(false);

                } else {
                    Ext.getCmp('printbtn').setDisabled(true);

                }
            }
        }
    });
    var StatusStore = Ext.create('Ext.data.Store', {
        fields: ['statusName', 'statusId'],
        data: [
            { statusName: '全部', statusId: 'all' },
            { statusName: 'Y-使用中', statusId: 'Y' },
            { statusName: 'N-停用', statusId: 'N' }
        ]
    });
    var StatusStore2 = Ext.create('Ext.data.Store', {
        fields: ['statusName', 'statusId'],
        data: [
            { statusName: 'Y-使用中', statusId: 'Y' },
            { statusName: 'N-停用', statusId: 'N' }
        ]
    });
    var XCATEGORYStore = Ext.create('Ext.data.Store', {
        fields: ['XCATEGORY', 'DESCRIPT'],
        autoLoad: true,
        proxy: {
            type: 'ajax',
            url: '/api/CB0011/GetXcategory',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }
    });
    var T1Name = "物流箱條碼";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '物流箱編號',
            name: 'BOXNO',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '物流箱條碼',
            name: 'BARCODE',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required',
            value: 'BOX',
            listeners: {
                'change': function () {
                    if (this.getValue().length < 3) {
                        this.setValue('BOX');
                    }
                }
            }
        }, {
            fieldLabel: '物流箱說明',
            name: 'DESCRIPT',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            xtype: 'combobox',
            name: 'XCATEGORY',
            fieldLabel: '條碼分類代碼',
            store: XCATEGORYStore,
            displayField: 'DESCRIPT',
            valueField: 'XCATEGORY',
            editable: false,
            readOnly: true
        }, {
            xtype: 'combobox',
            name: 'STATUS',
            fieldLabel: '使用代碼',
            store: StatusStore2,
            queryMode: 'local',
            displayField: 'statusName',
            valueField: 'statusId',
            value: 'Y',
            editable: false,
            readOnly: true
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true, handler: function () {
                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    if (btn === 'yes') {
                        T1Submit();
                    }
                });
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            var x = f.findField('x').getValue();
            var _url;
            if (x === 'I') { _url = '/api/CB0011/AddBox'; }
            if (x === 'U') { _url = '/api/CB0011/UpdateBox'; }
            f.submit({
                url: _url,
                success: function (form, action) {
                    myMask.hide();
                    var r = form.getRecord();
                    switch (x) {
                        case "I":
                            var v = form.getValues();
                            r.set(v);
                            BoxStore.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            form.updateRecord(r);
                            r.commit();
                            break;
                        case "D":
                            BoxStore.remove(r);
                            r.commit();
                            break;
                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('BOXNO').setReadOnly(true);
        f.findField('BARCODE').setReadOnly(true);
        f.findField('DESCRIPT').setReadOnly(true);
        f.findField('XCATEGORY').setReadOnly(true);
        f.findField('STATUS').setReadOnly(true);
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        RightTabs.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: BoxStore,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            var f = T1Form.getForm();
                            var boxno = f.findField('BOXNO').value;
                            Ext.Ajax.request({
                                url: '/api/CB0011/DelBox',
                                //method: reqVal_g,
                                params: {
                                    BOXNO: boxno
                                },
                                success: function (response, opts) {
                                    BoxStore.remove(T1LastRec);
                                    //T1LastRec.commit();
                                },
                                failure: function (response, opts) {
                                    alert(response.responseText);
                                    //alert(response.responseText);
                                }
                            });
                        }
                    });
                }
            }
        ]
    });
    function setFormT1(x, t) {
        RightTabs.setActiveTab(RightTabs.down('#form'));
        viewport.down('#t1Grid').mask();
        RightTabs.down('#form').setTitle(t + T1Name);
        viewport.down('#rightTab').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            T1Form.reset();
            u = f.findField("BOXNO");
            u.setReadOnly(false);
        }
        else {
            u = f.findField('BARCODE');
        }
        f.findField('x').setValue(x);
        f.findField('BARCODE').setReadOnly(false);
        f.findField('DESCRIPT').setReadOnly(false);
        f.findField('XCATEGORY').setReadOnly(false);
        f.findField('STATUS').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: BoxStore,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                xtype: 'toolbar',
                //style: 'background:white',
                items: [{
                    xtype: 'textfield',
                    fieldLabel: '物流箱編號',
                    width: 180,
                    labelWidth: 70,
                    id: 'BOXNO',
                    enforceMaxLength: true,
                    maxLength: 20
                }, {
                    xtype: 'textfield',
                    fieldLabel: '物流箱條碼',
                    width: 180,
                    labelWidth: 70,
                    id: 'BARCODE',
                    enforceMaxLength: true,
                    maxLength: 200,
                    value: 'BOX',
                    listeners: {
                        'change': function () {
                            if (this.getValue().length < 3) {
                                this.setValue('BOX');
                            }
                        }
                    }
                }, {
                    xtype: 'combobox',
                    id: 'STATUS',
                    labelWidth: 60,
                    width: 150,
                    fieldLabel: '使用代碼',
                    store: StatusStore,
                    queryMode: 'local',
                    displayField: 'statusName',
                    valueField: 'statusId',
                    value: 'all',
                    editable: false
                }, {
                    xtype: 'button',
                    width: 60,
                    text: '查詢',
                    handler: function () {
                        BoxStore.load();
                    }
                }, {
                    xtype: 'button',
                    width: 60,
                    text: '清除',
                    handler: function () {
                        Ext.getCmp('BOXNO').setValue(null);
                        Ext.getCmp('BARCODE').setValue(null);
                        Ext.getCmp('STATUS').setValue('all');
                        Ext.getCmp('BOXNO').focus();
                    }
                }, {
                        xtype: 'button',
                        id: 'printbtn',
                    disabled: true,
                    width: 60,
                    text: '列印',
                    handler: function () {
                        var BOXNO = Ext.getCmp('BOXNO').getValue();
                        var BARCODE = Ext.getCmp('BARCODE').getValue();
                        var STATUS = Ext.getCmp('STATUS').getValue();
                        showReport(BOXNO,BARCODE,STATUS);
                        //var printWin = window.open('', '', 'width=800,height=600');
                        //printWin.document.write(printStore(BoxStore, true));
                        //printWin.document.write("<script type='text/javascript'>$('div').each(function(i, ele) {var v = $(this).html();$(this).html('').show().barcode(v,'code128');});</script>");
                        //printWin.document.close();
                    }
                }
                ]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
        columns: [
            {
                xtype: 'rownumberer',
                width: 30,
                align: 'Center',
                labelAlign: 'Center'
            }, {
                text: '物流箱編號', dataIndex: 'BOXNO'
            }, {
                text: '物流箱條碼', dataIndex: 'BARCODE'
            }, {
                text: '物流箱說明', dataIndex: 'DESCRIPT'
            }, {
                text: '條碼分類代碼', dataIndex: 'XCATEGORY'
            }, {
                text: '使用代碼', dataIndex: 'STATUS', align: 'center'
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('BOXNO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T1Form.getForm().reset();
        }
    }

    var RightTabs = Ext.createWidget('tabpanel', {
        activeTabe: 0,
        plain: true,
        border: false,
        items: [
            {
                itemId: 'form',
                title: '瀏覽',
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'rightTab',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [RightTabs]
        }
        ]
    });
    Ext.getCmp('BOXNO').focus();

    //print function
    var head = false;
    var cols = 0;
    function printStore(store, th) {
        var a = store.data.items;
        var h = new Array();
        var d = new Array();
        var colname;
        for (var i = 0; i < a.length; i++) {
            for (var propName in a[i].data) {
                if (propName == 'ROWNUM' || propName == 'BOXNO' || propName == 'BARCODE' || propName == 'DESCRIPT' || propName == 'XCATEGORY' || propName == 'STATUS' || propName == 'BARCODE_ICON') {
                    if (propName == 'ROWNUM') { colname = '項次' }
                    if (propName == 'BOXNO') { colname = '物流箱編號' }
                    if (propName == 'BARCODE') { colname = '物流箱條碼' }
                    if (propName == 'DESCRIPT') { colname = '物流箱說明' }
                    if (propName == 'XCATEGORY') { colname = '條碼分類代碼' }
                    if (propName == 'STATUS') { colname = '使用代碼' }
                    if (propName == 'BARCODE_ICON') { colname = '條碼圖示' }
                    if (!(a[i].data[propName] instanceof Function)) {
                        if (propName == 'BARCODE_ICON') {
                            d.push('<div>' + a[i].data['BARCODE'] + '</div>');
                        } else {
                            d.push(a[i].data[propName]);
                        }
                        if (i < 1) {
                            h.push(colname);
                            cols++;
                        }
                    }
                }
            }
        };
        var col = 0;
        var rw = new Array();
        var rs = new Array();
        for (var i = 0; i < d.length; i++) {
            if (col < cols) {
                rw.push(d[i]);
                col++;
                if (col == cols) {
                    col = 0;
                    rs.push(rw);
                    rw = new Array();
                }
            }
        };
        var ret = "";
        if (th)
            ret = "<!DOCTYPE html><html><head><title>物流箱條碼列印</title><style>tr:nth-child(even) {background-color: #f2f2f2;}</style><script type='text/javascript' src='/Scripts/jquery-1.8.2.js'></script><script type='text/javascript' src='/Scripts/jquery-barcode.js'></script></head><body>";
        ret += t(h, rs, th);
        if (th)
            ret += "<p align='center'><input type='button' value='列印' onclick='window.print();'></p></body></html>";
        head = false;
        cols = 0;
        return ret;
    }

    function t(h, d, th) {
        if (th)
            return "<table style='border-collapse: collapse;' border='1' cellpadding=4 align='center'>" + tc(h, d, th) + "</table>";
        else
            return "<table>" + tc(h, d, th) + "</table>";
    }

    function tc(h, d, ih) {
        var ret = "";
        if (!head) {
            ret += "<tr align='center'>";
            for (var i = 0; i < h.length; i++) {
                if (ih)
                    ret += th(h[i]);
                else
                    ret += td(h[i]);
            };
            ret += "</tr>";
            head = true;
        }
        for (var i = 0; i < d.length; i++) {
            ret += "<tr align='center'>" + tdd(d[i]) + "</tr>";
        };
        return ret;
    }

    function tdd(d) {
        var ret = "";
        for (var i = 0; i < d.length; i++) {
            ret += td(d[i]);
        };
        return ret;
    }

    function td(d) {
        return "<td>" + d + "</td>";
    }

    function th(h) {
        return "<th>" + h + "</th>";
    }
    function showReport(BOXNO,BARCODE,STATUS) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="/Report/C/CB0011.aspx?BOXNO=' + BOXNO + '&BARCODE=' + BARCODE + '&STATUS=' + STATUS + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }
});