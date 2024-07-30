Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';

    var T3GetExcel = '../../../api/AB0137/Excel';
    var STOCKCODE = '';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];


    // 物品類別清單
    var Wh_noQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function SetWh_noQueryCombo() {
        Wh_noQueryStore.add({ VALUE: 'E70000', TEXT: 'E70000 成功嶺門診藥局' });
        Wh_noQueryStore.add({ VALUE: 'E70001', TEXT: 'E70001 成功嶺替代役藥局' });

    }

    SetWh_noQueryCombo();

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P3',
        fieldLabel: '院內碼',
        allowBlank: true,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0137/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {

        }
    });

    var T1Store = Ext.create('Ext.data.Store', {
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'USEDATE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0137/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {

        T1Tool.moveFirst();
    }



    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
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
                            xtype: 'datefield',
                            fieldLabel: '消耗日期',
                            name: 'P0',
                            labelWidth: mLabelWidth,
                            width: 150,
                            padding: '0 4 0 4',
                            value: new Date()
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            name: 'P1',
                            labelWidth: 8,
                            width: 88,
                            padding: '0 4 0 4',
                            labelSeparator: '',
                            value: new Date()
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '扣庫藥局',
                            name: 'P2',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            store: Wh_noQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        mmCodeCombo
                        ,
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                T1Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'getexport',
                name: 'getexport',
                text: '匯入',
                handler: function () {
                    msglabel('訊息區:');
                    showWin2();
                }
            }
        ]
    });

    Ext.define('selRecords', {
        selectarray: Array(),
        insertarray: function (key) {
            var checkexist = false;
            Ext.Array.each(selRecords.selectarray, function (id) {
                if (id == key) {
                    checkexist = true;
                    return;
                }
            });
            if (!checkexist) {
                selRecords.selectarray.push(key);
            }
        },
        deletearray: function (key) {
            selRecords.selectarray.splice(jQuery.inArray(key, selRecords.selectarray), 1)
        },
        singleton: true
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
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "消耗日期",
                dataIndex: 'USEDATE',
                width: 80,
                format: 'Xmd'
            },
            {
                text: "扣庫藥局",
                dataIndex: 'STOCKCODE',
                width: 110
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120

            },
            {
                text: "消耗量",
                dataIndex: 'USEQTY',
                style: 'text-align:left',
                width: 80,
                align: 'right'
            },
            {
                text: "計量單位代碼",
                dataIndex: 'BASE_UNIT',
                width: 120

            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 300

            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 120

            },
            {
                //xtype: 'datecolumn',
                text: "匯入時間",
                dataIndex: 'CREATE_TIME',
                width: 120
                //format: 'Xmd'
            },
            {
                header: "",
                flex: 1
            }
        ],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                }
            }
        },
        listeners: {

            selectionchange: function (model, records) {
            }
        }
    });


 






    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PR_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'PR_QTY', type: 'string' },
            { name: 'CHECK_RESULT', type: 'string' }
        ]
    });
    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    })

    function T3Load() {
        T3Store.load({
            params: {
                start: 0
            }
        });
    }
    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 'T3export',
                name: 'T3export',
                text: '下載匯入範本', handler: function () {
                    var p = new Array();
                    //p.push({ name: 'p0', value: T1LastRec.data.PR_NO });
                    PostForm(T3GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'T3send',
                id: 'T3send',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('T3insert').setDisabled(true);
                        T3Store.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('T3send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            //formData.append("pr_no", T1LastRec.data.PR_NO);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AB0137/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    var msg = data.msg.split(',');
                                    if (!data.success) {
                                        T3Store.removeAll();
                                        Ext.MessageBox.alert("提示", msg[0]);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T3insert').setDisabled(true);
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T3Store.loadData(data.etts, false);
                                        if (msg[0] == "True") {
                                            Ext.getCmp('T3insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。" + (msg[1] == "True" ? "<br/>檢核<span style=\"color: red; font-weight: bold\">金額大於15萬</span>。" : ""));
                                        };
                                        if (msg[0] == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。" + (msg[1] == "True" ? "<br/>檢核<span style=\"color: red; font-weight: bold\">金額大於15萬</span>。" : ""));
                                        };

                                    }
                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                    Ext.getCmp('T3insert').setDisabled(true);
                                    myMask.hide();
                                }
                            });
                        }
                    }
                }
            }
        ]
    });

    function T3Cleanup() {
        T3Query.getForm().reset();
        msglabel('訊息區:');
    }
    var T3Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            width: 250
        },
        {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 130,
            sortable: true

        }, {
            text: "消耗量",
            dataIndex: 'USEQTY',
            width: 100,
            style: 'text-align:left',
            align: 'right'

        }, {
            text: "物料類別",
            dataIndex: 'MAT_CLASS',
            width: 80
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 300
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120
        },
        {
            header: "",
            flex: 1
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
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;
    var win2;
    if (!win2) {
        win2 = Ext.widget('window', {
            title: '選擇扣庫藥局',
            closeAction: 'hide',
            width: 250,
            height: 200,
            layout: 'form',
            resizable: true,
            modal: true,
            constrain: true,
            items: [{
                xtype: 'combo',
                fieldLabel: '扣庫藥局',
                id: 'win2_STOCKCODE',
                enforceMaxLength: false,
                labelWidth: 60,
                width: 180,
                fieldCls: 'required',
                padding: '0 4 0 4',
                store: Wh_noQueryStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
            }],
            buttons: [{
                text: '確定',
                handler: function () {
                    STOCKCODE = win2.down('[id=win2_STOCKCODE]').getValue();
                    
                    if (STOCKCODE == '' || STOCKCODE == null) {
                        Ext.MessageBox.alert("提示", "請選擇<span style=\"color: red; font-weight: bold\">扣庫藥局</span>。");
                        return false;
                    }
                    else {
                        hideWin2();
                        showWin3();
                    }

                }
            }, {
                text: '取消',
                handler: function () {
                    hideWin2();
                }
            }],
            listeners: {
                //move: function (xwin, x, y, eOpts) {
                //    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                //    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                //},
                //resize: function (xwin, width, height) {
                //    winActWidth = width;
                //    winActHeight = height;
                //}
            }
        });
    }

    var win3;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '資料匯入檢核',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T3Grid],
            buttons: [{
                text: '確定匯入',
                id: 'T3insert',
                name: 'T3insert',
                disabled: true,
                handler: function () {
                    var rawData = Ext.pluck(T3Store.data.items, 'data');
                    var filteredData = rawData.map(function (item) {
                        var filteredItem = {
                            MMCODE: item.MMCODE,
                            USEQTY: item.USEQTY
                        };
                        return filteredItem;
                    });


                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AB0137/Insert',
                        method: reqVal_p,
                        params: {
                            //data: Ext.encode(Ext.pluck(T3Store.data.items, 'data')),
                            data: Ext.encode(filteredData),
                            STOCKCODE: STOCKCODE
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");
                                }
                                Ext.getCmp('T3insert').setDisabled(true);
                                T3Store.removeAll();
                                T1Load();
                            }
                            myMask.hide();
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
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });

                    hideWin3();
                }
            }, {
                text: '關閉',
                handler: function () {
                    hideWin3();
                }
            }],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }
    function showWin2() {
        if (win2.hidden) {
            win2.show();
        }
    }
    function hideWin2() {
        if (!win2.hidden) {
            win2.hide();
            var combo = win2.down('[id=win2_STOCKCODE]'); 
            combo.reset();
        }
    }

    function showWin3() {
        if (win3.hidden) {
            T3Cleanup();
            T3Store.removeAll();
            win3.setTitle('資料匯入檢核');
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
            T3Cleanup();
        }
    }

});