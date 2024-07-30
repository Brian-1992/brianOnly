Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var viewModel = Ext.create('WEBAPP.store.CE0014VM');
    var T1Name = '庫房盤點人員資料';
    var T2Name = '人員資料';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var viewModel = Ext.create('WEBAPP.store.CE0014VM');
    var T1Store = viewModel.getStore('BC_WHCHKID');

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('WH_NO').getValue());
        T1Tool.moveFirst();
    }

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: '/api/CE0002/GetWhnoCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    //                    whnoQueryStore.add({ WH_NO: '', WH_KIND: '', WH_NAME: '', WH_GRADE: '' });
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();
    
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 60
        },

        items: [{
            xtype: 'container',
            layout: 'hbox',
            //padding: '0 7 7 0',
            items: [
                {
                    xtype: 'combo',
                    store: whnoQueryStore,
                    name: 'WH_NO',
                    id: 'WH_NO',
                    fieldLabel: '庫房代碼',
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (!T1Query.getForm().findField('WH_NO').getValue())
                            Ext.MessageBox.alert('錯誤', '<span style="color:red">庫房代碼</span>為必填');
                        else {
                            T1Load();
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('WH_NO').focus();
                    }
                }
            ]
        }]
    });

    function setWhno(args) {
        if (args.WH_NO !== '') {
            T1Query.getForm().findField("WH_NO").setValue(args.WH_NO);
        }
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        items: [
            {
                itemId: 'delete',
                name: 'delete',
                text: '刪除',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    let wh_no = '';
                    //selection.map(item => {
                    //    wh_no += item.get('WH_NO') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        wh_no += item.get('WH_NO') + ',';
                    })
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            wh_no = T1Query.getForm().findField('WH_NO').getValue();
                            var data = getBcWhchkidItems(wh_no, selection, 'WH_CHKUID');

                            Ext.Ajax.request({
                                url: '/api/CE0014/Delete',
                                method: reqVal_p,
                                params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('資料刪除成功');
                                        T1Load();

                                        if (T2Query.getForm().findField('P0').getValue() != "" ||
                                            T2Query.getForm().findField('P1').getValue() != ""||
                                            T2Query.getForm().findField('INID').getValue() != "") {
                                            T2Load();
                                        }
                                       
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            })
                        }
                    })
                },
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }
        ],

        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',

        // grid columns
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80,
            sortable: true
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 160,
            sortable: true
        }, {
            text: "盤點人員代碼",
            dataIndex: 'WH_CHKUID',
            width: 100,
            sortable: true
        }, {
                text: "盤點人員姓名",
                dataIndex: 'WH_CHKUID_NAME',
            width: 100,
            sortable: true
        },  {
            text: "庫房分類",
            dataIndex: 'WH_KIND',
            width: 80,
            sortable: true
        }, {
            text: "庫房級別",
            dataIndex: 'WH_GRADE',
            width: 80,
            sortable: true
        }, {
            header: "",
            flex: 1
        }],

        listeners: {
            selectionchange: function (model, records) {
                T1Grid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    T1Form.loadRecord(records[0]);
                }
            }
        }
    });

    var T2Store = viewModel.getStore('CE0014_URID');
    function T2Load() {
        T2Store.getProxy().setExtraParam("p0", T2Query.getForm().findField('P0').getValue());
        T2Store.getProxy().setExtraParam("p1", T2Query.getForm().findField('P1').getValue());
        T2Store.getProxy().setExtraParam("p2", T2Query.getForm().findField('INID').getValue());
        T2Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('WH_NO').getValue());
        T2Tool.moveFirst();
    }

    var InidCombo = Ext.create('WEBAPP.form.InidWhCombo', {
        name: 'INID',
        fieldLabel: '責任中心代碼',
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/CE0014/GetInidCombo',

        //查詢完會回傳的欄位
        //extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            //var f = T2Form.getForm();
            //if (!f.findField("MMCODE").readOnly) {
            //    tmpArray = f.findField("FRWH2").getValue().split(' ');
            //    return {
            //        //MMCODE: f.findField("MMCODE").getValue(),
            //        WH_NO: tmpArray[0]
            //    };
            //}
        },
        listeners: {
            select: function (c, r, i, e) {

                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                //var f = T2Form.getForm();
                if (r.get('INID') !== '') {
                    T2Query.getForm().findField("INID").setValue(r.get('INID'));
                }
            }
        }
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 80
        },

        items: [
            InidCombo,
             {
            fieldLabel: '中文姓名',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 60,
            width: 200,
            padding: '0 4 0 4'
            },
             {
                 fieldLabel: 'AD帳號',
                 name: 'P0',
                 enforceMaxLength: true,
                 maxLength: 60,
                 width: 200,
                 labelWidth: 60,
                 padding: '0 4 0 4',
                 hidden:true
             },
        {
            xtype: 'button',
            text: '查詢',
            handler: function () {
                if (!T1Query.getForm().findField('WH_NO').getValue()) {
                    Ext.Msg.alert('錯誤', '請先選擇庫房代碼');
                    return;
                }
                if (!T2Query.getForm().findField('P0').getValue() &&
                    !T2Query.getForm().findField('P1').getValue() &&
                    !T2Query.getForm().findField('INID').getValue()) {
                    Ext.MessageBox.alert('錯誤', '請至少輸入一項查詢條件資料');
                }
                else {
                    T2Load();
                }
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
            }
        }]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'insert',
                name: 'insert',
                text: '指定',
                disabled: true,
                handler: function () {
                    if (!T1Query.getForm().findField('WH_NO').getValue()) {
                        Ext.MessageBox.alert('錯誤', '尚未選取<span style="color:red">庫房代碼</span>');
                        return;
                    }

                    var selection = T2Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.MessageBox.alert('錯誤', '尚未選取');
                        return;
                    }

                    let name = '';
                    let wh_no = T1Query.getForm().findField('WH_NO').getValue();
                    //selection.map(item => {
                    //    name += '「' + item.get('TUSER') + '」<br>';
                    //});
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('TUSER') + '」<br>';
                    })

                    Ext.MessageBox.confirm('指定', '確認是否指定庫房代碼 ' + wh_no + ' 至盤點人員<br>' + name, function (btn, text) {
                        if (btn === 'yes') {

                            var data = getBcWhchkidItems(wh_no, selection, 'TUSER');

                            Ext.Ajax.request({
                                url: '/api/CE0014/Create',
                                method: reqVal_p,
                                params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('指定庫房代碼成功');
                                        T1Load();
                                        T2Load();
                                    }
                                    else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                        T1Load();
                                        T2Load();
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                },
                            });
                        }
                    });
                }
            }
        ]
    });
    var getBcWhchkidItems = function (wh_no, selection, idField) {
        var temp = [];
        for (var i = 0; i < selection.length; i++) {
            var item = {
                WH_NO: wh_no,
                WH_CHKUID: selection[i].data[idField]
            }
            temp.push(item);
        }
        return temp;
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: T2Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T2Tool]
        }
        ],

        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',

        // grid columns
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "人員代碼",
            dataIndex: 'TUSER',
            width: 80,
            sortable: true
        }, {
            text: "中文姓名",
            dataIndex: 'UNA',
            width: 80,
            sortable: true
        }, {
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 100,
            sortable: true
        }, {
            text: "責任中心名稱",
            dataIndex: 'INID_NAME',
            width: 300,
            sortable: true
        }, {
            header: "",
            flex: 1
        }],

        listeners: {
            selectionchange: function (model, records) {
                T2Grid.down('#insert').setDisabled(records.length === 0);
                //if (records[0]) {
                //    T2Form.loadRecord(records[0]);
                //}
            }
        }
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            height: '50%',
            items: [T1Grid]
        }, {
            itemId: 't2Grid',
            region: 'south',
            layout: 'fit',
            split: true,
            collapsible: true,
            border: false,
            height: '50%',
            items: [T2Grid]
        }]
    });
});





