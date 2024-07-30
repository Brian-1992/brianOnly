Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = '中央庫房個人完成盤點';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var windowHeight = $(window).height();

    var ST_Date = '';
    var YYYMM = '';
    var pLevel = '';
    var T1LastRec = null;
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var fid = Ext.getUrlParam('LEVEL');

    function SetLevel() {
        if (fid == '1') {
            pLevel = '初盤';
        }
        else if (fid == '2') {
            pLevel = '複盤';
        }
        else {
            pLevel = '三盤';
        }
    }

    var StoreidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     
            { "VALUE": "0", "TEXT": "非庫備" },
            { "VALUE": "1", "TEXT": "庫備" },
            { "VALUE": "3", "TEXT": "小額採購" }
        ]
    });
    var MatclassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     
            { "VALUE": "02", "TEXT": "衛材" },
            { "VALUE": "07", "TEXT": "被服" },
            { "VALUE": "08", "TEXT": "資訊耗材" },
            { "VALUE": "0X", "TEXT": "一般物品" }
        ]
    });


    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CHK_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'WH_NO', type: 'string' },
            { name: 'STORE_LOC', type: 'string' },
            { name: 'LOC_NAME', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            { name: 'STORE_QTYC', type: 'string' },
            { name: 'STORE_QTYM', type: 'string' },
            { name: 'STORE_QTYS', type: 'string' },
            { name: 'CHK_QTY', type: 'string' },
            { name: 'CHK_REMARK', type: 'string' },
            { name: 'CHK_UID', type: 'string' },
            { name: 'CHK_TIME', type: 'string' },
            { name: 'STATUS_INI', type: 'string' },
            { name: 'CREATE_DATE', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'DONE_NUM', type: 'string' },
            { name: 'UNDONE_NUM', type: 'string' },
            { name: 'DONE_STATUS', type: 'string' },
            { name: 'CHK_UID_NAME', type: 'string' },
            { name: 'CHK_TIME_T', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 50,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: fid,
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: '/api/CE0029/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    T1Store.on('load', function (store, records) {
        
        Ext.getCmp('btnUpdate').disable();
        Ext.getCmp('btnFinish').disable();

        if (store.data.items.length == 0) {
            Ext.Msg.alert('提醒', '無盤點品項');
            return;
        }

        for (var i = 0; i < store.data.items.length; i++) {
            if (store.data.items[i].data.STATUS_INI == '1') {
                Ext.getCmp('btnUpdate').enable();
                Ext.getCmp('btnFinish').enable();
            } 
        }
    });
    function T1Load() {
        T1Store.removeAll();
        msglabel('訊息區:');
        Ext.Ajax.request({
            url: '/api/CE0029/CheckUserInChkuid',
            method: reqVal_p,
            params: {
                chk_level: fid,
                chk_type: T1Query.getForm().findField('P1').getValue(),
                chk_class: T1Query.getForm().findField('P2').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.msg != '') {
                    Ext.Msg.alert('提醒', data.msg);
                    return; 
                }
                Ext.Ajax.request({
                    url: '/api/CE0029/CheckPDADone',
                    method: reqVal_p,
                    params: {
                        chk_level: fid,
                        chk_type: T1Query.getForm().findField('P1').getValue(),
                        chk_class: T1Query.getForm().findField('P2').getValue()
                    },
                    success: function (response) {
                        var data1 = Ext.decode(response.responseText);
                        if (data1.msg != '') {
                            Ext.Msg.alert('提醒', data1.msg);
                            return;
                        }
                        T1Tool.moveFirst();
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('失敗', '發生例外錯誤');
                    }
                });
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        }); 
    }
    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 180;

    function GetYyymm() {
        ST_Date = new Date();
        ST_Date = Ext.Date.format(ST_Date, "Ymd") - 19110000;
        YYYMM = ST_Date.toString().substring(0, 5);
    }
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '月份',
                            name: 'PMON',
                            id: 'PMON',
                            labelWidth: 60,
                            width: 130,
                            readOnly: true,
                            submitValue: true
                        },{
                            xtype: 'combo',
                            store: StoreidStore,
                            name: 'P1',
                            id: 'P1',
                            fieldLabel: '盤點類別',
                            labelAlign: 'right',
                            labelWidth: mLabelWidth,
                            width: mWidth,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false, // 欄位為必填
                            typeAhead: true,
                            forceSelection: true,
                            queryMode: 'local',
                            triggerAction: 'all',
                            fieldCls: 'required'
                        },
                        {
                            xtype: 'combo',
                            store: MatclassStore,
                            name: 'P2',
                            id: 'P2',
                            fieldLabel: '物料分類',
                            labelAlign: 'right',
                            labelWidth: mLabelWidth,
                            width: mWidth,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false, // 欄位為必填
                            typeAhead: true,
                            forceSelection: true,
                            queryMode: 'local',
                            triggerAction: 'all',
                            fieldCls: 'required'
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點階段',
                            labelAlign: 'right',
                            labelWidth: mLabelWidth,
                            width: mWidth,
                            name: 'P3',
                            id: 'P3'
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                T1Load();
                                msglabel('訊息區:');
                                
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P1').focus(); // 進入畫面時輸入游標預設在PMON
                                T1Query.getForm().findField('PMON').setValue(YYYMM);
                                T1Query.getForm().findField('P1').setValue('1');
                                T1Query.getForm().findField('P2').setValue('02');
                                T1Query.getForm().findField('P3').setValue(pLevel);
                            }
                        }
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
             {
                text: '儲存',
                id: 'btnUpdate',
                name: 'btnUpdate',
                disabled: true,
                handler: function () {
                    var tempData = T1Grid.getStore().data.items;
                    var data = [];
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }
                    
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();

                    Ext.Ajax.request({
                        url: '/api/CE0029/UpdateChkd',
                        method: reqVal_p,
                        contentType: "application/json",
                        params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                        success: function (response) {

                            var data = JSON.parse(response.responseText);
                            myMask.hide();
                            if (data.success) {

                                if (data.success == false) {
                                    Ext.Msg.alert('失敗', data.msg);
                                    return;
                                }

                                msglabel('訊息區:資料儲存成功');
                                T1Tool.moveFirst();
                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        },

                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            }, {
                text: '完成盤點輸入',
                id: 'btnFinish',
                name: 'btnFinish',
                disabled: true,
                handler: function () {
                    var chk_no = T1Grid.getStore().data.items[0].data.CHK_NO;

                    Ext.Ajax.request({
                        url: '/api/CE0029/FinishChkd',
                        method: reqVal_p,
                        params: {
                            chk_no: chk_no
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('完成盤點輸入');
                                T1Store.load({
                                    params: {
                                        start: 0
                                    }
                                });
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }


        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        //selModel: Ext.create('Ext.selection.CheckboxModel', {
        //    checkOnly: true,
        //    injectCheckbox: 'first',
        //    mode: 'SIMPLE',
        //    showHeaderCheckbox: true
        //}),
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 200
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "<span style='color:red'>盤點量</span>",
                dataIndex: 'CHK_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80,
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/,
                    //fieldCls: 'required',
                    selectOnFocus: true,
                    //allowBlank: false,
                    //minValue: 0,
                    //hideTrigger: true,
                    //keyNavEnabled: false,
                    //mouseWheelEnabled: false,
                    //listeners: {
                    //    specialkey: function (field, e) {
                    //        if (e.getKey() === e.UP) {
                    //            arrow = 'UP';
                    //            e.preventDefault();
                    //            var editPlugin = this.up().editingPlugin;
                    //            editPlugin.completeEdit();
                    //            //var sm = T1Grid.getSelectionModel();
                    //            //sm.deselectAll();
                    //            //sm.select(editPlugin.context.rowIdx - 1);
                    //            editPlugin.startEdit(editPlugin.context.rowIdx - 1, 6);
                    //        }

                    //        if (e.getKey() === e.DOWN || e.getKey() === e.ENTER) {
                    //            arrow = 'DOWN';
                    //            e.preventDefault();
                    //            var editPlugin = this.up().editingPlugin;
                    //            editPlugin.completeEdit();
                    //            //var sm = T1Grid.getSelectionModel();
                    //            //sm.deselectAll();
                    //            //sm.select(editPlugin.context.rowIdx + 1);
                    //            editPlugin.startEdit(editPlugin.context.rowIdx + 1, 6);
                    //        }
                    //    }
                    //}
                }
            }, {
                text: "盤點時間",
                dataIndex: 'CHK_TIME_T',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                header: "",
                flex: 1
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    
                }
            })
        ],
        listeners: {
            beforeedit: function (context, eOpts) {
                // 編輯前紀錄要用到的值(使用者採用點其他列的資料完成編輯會導致T1LastRec被更新)
                if (T1LastRec.data.CHK_TIME == '') {
                    return false;
                }
                if (T1LastRec.data.STATUS_INI == '2') {
                    return false;
                }
                
                    return true;
                
            },
            validateedit: function (editor, context, eOpts) {

            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                Ext.getCmp('btnUpdate').setDisabled(true);
                if (records.length > 0) {
                    T1LastRec = Ext.clone(records[0]);
                    if (T1LastRec.data.STATUS_INI == '1') {
                        Ext.getCmp('btnUpdate').setDisabled(false);
                    }
                }
            },
            cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {
                T1LastRec = record;
            }
        },
        viewConfig: {
            listeners: {
                itemkeydown: function (grid, rec, item, idx, e) {
                    if (e.keyCode == 38) { //上
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx - 1, editPlugin.context.colIdx);
                    } else if (e.keyCode == 40) { //下
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx + 1, editPlugin.context.colIdx);
                    }

                    //if (e.keyCode == 13) { //enter
                    //    T4Load(currentChknos, false);
                    //}

                    //alert('The press key is' + e.getKey());
                }
            }
        },
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
        items: [
            {
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            }
        ]
    });

    GetYyymm();
    SetLevel();
    T1Query.getForm().findField('PMON').setValue(YYYMM);
    T1Query.getForm().findField('P1').setValue('1');
    T1Query.getForm().findField('P2').setValue('02');
    T1Query.getForm().findField('P3').setValue(pLevel);

});